package main

import (
    "context"
    "fmt"
    "log/slog"
    "net/http"
    "os"
    "os/signal"
    "syscall"
    "time"

    "streamerrio-backend/internal/config"
    "streamerrio-backend/internal/handler"
    "streamerrio-backend/internal/repository"
    "streamerrio-backend/internal/service"
    "streamerrio-backend/pkg/counter"
    "streamerrio-backend/pkg/logger"

    "github.com/jmoiron/sqlx"
    "github.com/joho/godotenv"
    "github.com/labstack/echo/v4"
    "github.com/labstack/echo/v4/middleware"
    _ "github.com/lib/pq"
    "github.com/redis/go-redis/v9"
)

func main() {
    // 環境変数読み込み
    _ = godotenv.Load()

    // ロガー初期化
    logLevel := getEnv("LOG_LEVEL", "info")
    logFormat := getEnv("LOG_FORMAT", "json")
    log, err := logger.Init(logger.Config{
        Level:     logLevel,
        Format:    logFormat,
        AddSource: true,
    })
    if err != nil {
        fmt.Fprintf(os.Stderr, "Failed to initialize logger: %v\n", err)
        os.Exit(1)
    }

    log.Info("🚀 Starting Streamerrio Server",
        slog.String("log_level", logLevel),
        slog.String("log_format", logFormat))

    // アプリケーション設定ロード
    cfg, err := config.Load()
    if err != nil {
        log.Error("Failed to load config", slog.Any("error", err))
        os.Exit(1)
    }
    log.Info("✅ Configuration loaded",
        slog.String("port", cfg.Port),
        slog.String("db", maskPassword(cfg.DatabaseURL)),
        slog.String("redis", cfg.RedisURL))

    // データベース接続
    db, err := sqlx.Connect("postgres", cfg.DatabaseURL)
    if err != nil {
        log.Error("Failed to connect to database", slog.Any("error", err))
        os.Exit(1)
    }
    defer func() {
        if err := db.Close(); err != nil {
            log.Error("Failed to close database", slog.Any("error", err))
        }
    }()
    db.SetMaxOpenConns(25)
    db.SetMaxIdleConns(5)
    db.SetConnMaxLifetime(5 * time.Minute)
    log.Info("✅ Database connected",
        slog.Int("max_open_conns", 25),
        slog.Int("max_idle_conns", 5))

    // Redis接続
    rdb := redis.NewClient(&redis.Options{
        Addr:         cfg.RedisURL,
        PoolSize:     10,
        MinIdleConns: 2,
    })
    ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
    defer cancel()
    if err := rdb.Ping(ctx).Err(); err != nil {
        log.Error("Failed to connect to Redis", slog.Any("error", err))
        os.Exit(1)
    }
    defer func() {
        if err := rdb.Close(); err != nil {
            log.Error("Failed to close Redis", slog.Any("error", err))
        }
    }()
    log.Info("✅ Redis connected", slog.String("addr", cfg.RedisURL))

    // リポジトリ層初期化
    eventRepo := repository.NewEventRepository(db, log)
    roomRepo := repository.NewRoomRepository(db, log)
    viewerRepo := repository.NewViewerRepository(db, log)
    log.Info("✅ Repositories initialized")

    // カウンター初期化
    redisCounter := counter.NewRedisCounter(rdb, log)
    log.Info("✅ Counter initialized", slog.String("type", "redis"))

    // ゲーム設定マネージャー初期化
    gameConfig := config.NewGameConfig(log)
    log.Info("✅ Game config initialized",
        slog.Int("event_types", len(gameConfig.ListEventTypes())))

    // サービス層初期化
    roomService := service.NewRoomService(roomRepo, cfg)
    viewerService := service.NewViewerService(viewerRepo, log)

    // WebSocketハンドラ初期化
    wsHandler := handler.NewWebSocketHandler(roomService, log)
    
    // イベントサービス初期化（WebSocket送信者として注入）
    eventService := service.NewEventService(
        redisCounter,
        eventRepo,
        wsHandler,
        gameConfig,
        log,
    )

    // ゲームセッションサービス初期化
    sessionService := service.NewGameSessionService(
        roomService,
        eventRepo,
        viewerRepo,
        redisCounter,
        wsHandler,
        log,
    )

    // APIハンドラ初期化
    apiHandler := handler.NewAPIHandler(
        roomService,
        eventService,
        viewerService,
        sessionService,
        log,
    )

    // Echo初期化
    e := echo.New()
    e.HideBanner = true
    e.HidePort = true

    // ミドルウェア設定
    e.Use(middleware.RequestLoggerWithConfig(middleware.RequestLoggerConfig{
        LogStatus:   true,
        LogURI:      true,
        LogError:    true,
        LogMethod:   true,
        LogLatency:  true,
        HandleError: true,
        LogValuesFunc: func(c echo.Context, v middleware.RequestLoggerValues) error {
            if v.Error != nil {
                log.Error("Request failed",
                    slog.String("method", v.Method),
                    slog.String("uri", v.URI),
                    slog.Int("status", v.Status),
                    slog.Duration("latency", v.Latency),
                    slog.Any("error", v.Error))
            } else {
                log.Info("Request handled",
                    slog.String("method", v.Method),
                    slog.String("uri", v.URI),
                    slog.Int("status", v.Status),
                    slog.Duration("latency", v.Latency))
            }
            return nil
        },
    }))
    e.Use(middleware.Recover())
    e.Use(middleware.CORSWithConfig(middleware.CORSConfig{
        AllowOrigins: []string{cfg.FrontendURL, "*"},
        AllowMethods: []string{http.MethodGet, http.MethodPost, http.MethodPut, http.MethodOptions},
        AllowHeaders: []string{"ngrok-skip-browser-warning", echo.HeaderContentType, echo.HeaderAuthorization},
    }))

    // ルーティング設定
    e.GET("/", healthCheck(log))
    e.GET("/ws-unity", wsHandler.HandleUnityConnection)
    e.GET("/clients", wsHandler.ListClients)

    api := e.Group("/api")
    api.GET("/rooms/:id", apiHandler.GetRoom)
    api.POST("/rooms/:id/events", apiHandler.SendEvent)
    api.GET("/rooms/:id/stats", apiHandler.GetRoomStats)
    api.GET("/rooms/:id/results", apiHandler.GetRoomResults)
    api.GET("/rooms/:id/viewers/:viewer_id", apiHandler.GetViewerSummary)
    api.POST("/rooms/:id/end", apiHandler.EndGame)

    // 管理API（Basic認証）
    adminGroup := e.Group("/admin")
    adminUser := getEnv("ADMIN_USER", "admin")
    adminPass := getEnv("ADMIN_PASSWORD", "secret")
    adminGroup.Use(middleware.BasicAuth(func(username, password string, c echo.Context) (bool, error) {
        if username == adminUser && password == adminPass {
            return true, nil
        }
        return false, nil
    }))
    adminGroup.GET("/config", func(c echo.Context) error {
        return c.JSON(http.StatusOK, gameConfig.GetAllConfigs())
    })
    adminGroup.PUT("/config/:event_type", func(c echo.Context) error {
        var req struct {
            BaseThreshold int     `json:"base_threshold"`
            MinThreshold  int     `json:"min_threshold"`
            MaxThreshold  int     `json:"max_threshold"`
        }
        if err := c.Bind(&req); err != nil {
            return c.JSON(http.StatusBadRequest, map[string]string{"error": "invalid body"})
        }
        eventType := c.Param("event_type")
        if err := gameConfig.UpdateEventConfig(eventType, req.BaseThreshold, req.MinThreshold, req.MaxThreshold); err != nil {
            return c.JSON(http.StatusBadRequest, map[string]string{"error": err.Error()})
        }
        log.Info("Event config updated via admin API",
            slog.String("event_type", eventType),
            slog.Int("base", req.BaseThreshold))
        return c.JSON(http.StatusOK, map[string]string{"status": "updated"})
    })
    adminGroup.PUT("/viewer-multipliers", func(c echo.Context) error {
        var req []config.ViewerMultiplier
        if err := c.Bind(&req); err != nil {
            return c.JSON(http.StatusBadRequest, map[string]string{"error": "invalid body"})
        }
        if err := gameConfig.SetViewerMultipliers(req); err != nil {
            return c.JSON(http.StatusBadRequest, map[string]string{"error": err.Error()})
        }
        log.Info("Viewer multipliers updated via admin API",
            slog.Int("count", len(req)))
        return c.JSON(http.StatusOK, map[string]string{"status": "updated"})
    })

    log.Info("✅ Routes registered")

    // グレースフルシャットダウン設定
    go func() {
        if err := e.Start(":" + cfg.Port); err != nil && err != http.ErrServerClosed {
            log.Error("Server failed", slog.Any("error", err))
            os.Exit(1)
        }
    }()

    log.Info("🎮 Server listening", slog.String("port", cfg.Port))

    // シグナル待機
    quit := make(chan os.Signal, 1)
    signal.Notify(quit, os.Interrupt, syscall.SIGTERM)
    <-quit

    log.Info("🛑 Shutdown signal received")

    ctx, cancel = context.WithTimeout(context.Background(), 30*time.Second)
    defer cancel()

    if err := e.Shutdown(ctx); err != nil {
        log.Error("Server shutdown failed", slog.Any("error", err))
    }

    log.Info("👋 Server stopped gracefully")
}

func healthCheck(log *slog.Logger) echo.HandlerFunc {
    return func(c echo.Context) error {
        log.Debug("Health check", slog.String("ip", c.RealIP()))
        return c.JSON(http.StatusOK, map[string]string{
            "status":  "ok",
            "service": "streamerrio",
            "version": "2.0.0",
        })
    }
}

func getEnv(key, def string) string {
    if v := os.Getenv(key); v != "" {
        return v
    }
    return def
}

func maskPassword(dsn string) string {
    // 簡易マスキング（パスワード部分を***に置換）
    // 例: "host=localhost password=secret" -> "host=localhost password=***"
    if len(dsn) > 50 {
        return dsn[:50] + "..."
    }
    return dsn
}