package main

import (
	stdlog "log"
	"net/http"

	"streamerrio-backend/internal/config"
	"streamerrio-backend/internal/handler"
	"streamerrio-backend/internal/repository"
	"streamerrio-backend/internal/service"
	"streamerrio-backend/pkg/counter"

	// PostgreSQLドライバーを使用
	_ "github.com/lib/pq"
	"github.com/jmoiron/sqlx"
	"github.com/joho/godotenv"
	"github.com/labstack/echo/v4"
	"github.com/labstack/echo/v4/middleware"
	elog "github.com/labstack/gommon/log"
	"github.com/redis/go-redis/v9"
)

func main() {
    // 環境変数読み込み
    godotenv.Load()

    cfg, err := config.Load()
    if err != nil {
        stdlog.Fatal("Failed to load config:", err)
    }

    // データベース接続
    db, err := sqlx.Connect("postgres", cfg.DatabaseURL)
    if err != nil {
        stdlog.Fatal("Failed to connect to database:", err)
    }
    defer db.Close()

    // Redis クライアント & カウンター初期化
    rdb := redis.NewClient(&redis.Options{Addr: cfg.RedisURL})
    redisCounter := counter.NewRedisCounter(rdb)

    // リポジトリ初期化
    eventRepo := repository.NewEventRepository(db)
    roomRepo := repository.NewRoomRepository(db)

    // サービス初期化
    roomService := service.NewRoomService(roomRepo, cfg)

    // WebSocketハンドラーを初期化し、サービスに注入
    // 既存のグローバル WebSocket ハンドラを使用 (仕様: websocket.go は編集しない)
    wsHandler := handler.Handler
    // アダプタ (sendEventToUnity を公開インタフェースに合わせる)
    sender := webSocketAdapter{ws: wsHandler}
    eventService := service.NewEventService(redisCounter, eventRepo, sender)

    // APIハンドラー初期化
    apiHandler := handler.NewAPIHandler(roomService, eventService)

    // Echo初期化
    e := echo.New()
    e.Logger.SetLevel(elog.DEBUG)
    e.Use(middleware.Logger())
    e.Use(middleware.Recover())

    // CORS設定
    e.Use(middleware.CORSWithConfig(middleware.CORSConfig{
        AllowOrigins: []string{cfg.FrontendURL, "*"},
        AllowMethods: []string{http.MethodGet, http.MethodPost, http.MethodOptions},
        AllowHeaders: []string{"ngrok-skip-browser-warning", echo.HeaderContentType},
    }))

    // ルーティング設定
    e.GET("/", healthCheck)

    // WebSocketエンドポイント
    e.GET("/ws-unity", wsHandler.HandleUnityConnection)
    e.GET("/clients", wsHandler.ListClients)

    // 新規APIエンドポイント
    api := e.Group("/api")
    api.GET("/rooms/:id", apiHandler.GetRoom)
    api.POST("/rooms/:id/events", apiHandler.SendEvent)
    api.GET("/rooms/:id/stats", apiHandler.GetRoomStats)

    // サーバー起動
    stdlog.Printf("🚀 Streamerrio Server starting on port %s", cfg.Port)
    e.Logger.Fatal(e.Start(":" + cfg.Port))
}

func healthCheck(c echo.Context) error {
	return c.JSON(http.StatusOK, map[string]string{
		"status":  "ok",
		"service": "streamerrio",
		"version": "1.0.0",
	})
}

// webSocketAdapter adapts legacy handler to WebSocketSender interface
type webSocketAdapter struct { ws *handler.WebSocketHandler }
func (a webSocketAdapter) SendEventToUnity(roomID string, payload map[string]interface{}) error {
    return a.ws.SendEventToUnity(roomID, payload)
}
