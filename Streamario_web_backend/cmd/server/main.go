package main

import (
	stdlog "log"
	"net/http"
	"strings"

	"streamerrio-backend/internal/config"
	"streamerrio-backend/internal/handler"
	"streamerrio-backend/internal/repository"
	"streamerrio-backend/internal/service"
	"streamerrio-backend/pkg/counter"

	// PostgreSQLドライバー
	"github.com/jmoiron/sqlx"
	"github.com/joho/godotenv"
	"github.com/labstack/echo/v4"
	"github.com/labstack/echo/v4/middleware"
	elog "github.com/labstack/gommon/log"
	_ "github.com/lib/pq"
	"github.com/redis/go-redis/v9"
)

func main() {
	// 1. 環境変数読み込み (.env があれば適用)
	godotenv.Load()

	// 2. 設定ロード
	cfg, err := config.Load()
	if err != nil {
		stdlog.Fatal("Failed to load config:", err)
	}

	// 3. DB 接続確立
	db, err := sqlx.Connect("postgres", cfg.DatabaseURL)
	if err != nil {
		stdlog.Fatal("Failed to connect to database:", err)
	}
	defer db.Close()

	// 4. Redis 初期化 & カウンタ (イベント数 / 視聴者アクティビティ)
	var rdb *redis.Client
	if strings.HasPrefix(cfg.RedisURL, "redis://") || strings.HasPrefix(cfg.RedisURL, "rediss://") {
		opt, err := redis.ParseURL(cfg.RedisURL)
		if err != nil {
			stdlog.Fatal("Invalid REDIS_URL:", err)
		}
		rdb = redis.NewClient(opt)
	} else {
		rdb = redis.NewClient(&redis.Options{Addr: cfg.RedisURL})
	}
	redisCounter := counter.NewRedisCounter(rdb)

	// 5. リポジトリ (永続層) 準備
	eventRepo := repository.NewEventRepository(db)
	roomRepo := repository.NewRoomRepository(db)

	// 6. サービス層生成
	roomService := service.NewRoomService(roomRepo, cfg)
	// 既存グローバルではなく新規インスタンスを生成し RoomService を注入
	wsHandler := handler.NewWebSocketHandler()
	wsHandler.SetRoomService(roomService)
	sender := webSocketAdapter{ws: wsHandler}
	eventService := service.NewEventService(redisCounter, eventRepo, sender)
	apiHandler := handler.NewAPIHandler(roomService, eventService)

	// 7. Echo フレームワーク初期化 & ミドルウェア
	e := echo.New()
	e.Logger.SetLevel(elog.DEBUG)
	e.Use(middleware.Logger())  // アクセスログ
	e.Use(middleware.Recover()) // パニック回復

	// 8. CORS 設定 (暫定で * を許容 / TODO: 本番は限定)
	e.Use(middleware.CORSWithConfig(middleware.CORSConfig{
		AllowOrigins: []string{cfg.FrontendURL, "*"},
		AllowMethods: []string{http.MethodGet, http.MethodPost, http.MethodOptions},
		AllowHeaders: []string{"ngrok-skip-browser-warning", echo.HeaderContentType},
	}))

	// 9. ルーティング定義
	e.GET("/", healthCheck)
	// WebSocket
	e.GET("/ws-unity", wsHandler.HandleUnityConnection)
	e.GET("/clients", wsHandler.ListClients)
	// REST API
	api := e.Group("/api")
	api.GET("/rooms/:id", apiHandler.GetRoom)
	api.POST("/rooms/:id/events", apiHandler.SendEvent)
	api.GET("/rooms/:id/stats", apiHandler.GetRoomStats)

	// 10. サーバ起動
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

// webSocketAdapter: 既存 WebSocketHandler をサービス側インタフェースに適合させる薄いアダプタ
type webSocketAdapter struct{ ws *handler.WebSocketHandler }

func (a webSocketAdapter) SendEventToUnity(roomID string, payload map[string]interface{}) error {
	return a.ws.SendEventToUnity(roomID, payload)
}
