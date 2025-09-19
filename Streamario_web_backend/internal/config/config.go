package config

import (
	"fmt"
	"os"
)

// Config: アプリケーション全体の設定値コンテナ
// 取得元は基本的に環境変数。存在しない項目はデフォルトを適用。
type Config struct {
	Port        string // APIサーバ待受ポート
	FrontendURL string // CORS 許可先 ("*" は全許可)
	DatabaseURL string // PostgreSQL 接続 DSN or URL
	RedisURL    string // Redis アドレス (host:port)
}

// Load: 環境変数から設定を組み立て (不足はデフォルト補完)
// 優先順位:
//  1. 直接 URL (DATABASE_URL / REDIS_URL)
//  2. 個別値を合成 (DB_HOST, DB_PORT, ...)
//  3. デフォルト (localhost 等)
func Load() (*Config, error) {
	cfg := &Config{}

	// Port
	cfg.Port = getEnv("PORT", "8888")

	// Frontend (CORS)
	cfg.FrontendURL = getEnv("FRONTEND_URL", "*")

	// Database URL
	if url := os.Getenv("DATABASE_URL"); url != "" {
		cfg.DatabaseURL = url
	} else {
		host := getEnv("DB_HOST", "localhost")
		port := getEnv("DB_PORT", "5432")
		user := getEnv("DB_USER", "postgres")
		pass := getEnv("DB_PASSWORD", "postgres")
		name := getEnv("DB_NAME", "streamerio")
		cfg.DatabaseURL = fmt.Sprintf("host=%s port=%s user=%s password=%s dbname=%s sslmode=disable", host, port, user, pass, name)
	}

	// Redis URL (addr only)
	if rurl := os.Getenv("REDIS_URL"); rurl != "" {
		cfg.RedisURL = rurl
	} else if addr := os.Getenv("REDIS_ADDR"); addr != "" { // fallback key
		cfg.RedisURL = addr
	} else {
		cfg.RedisURL = "localhost:6379"
	}

	return cfg, nil
}

func getEnv(key, def string) string {
	if v := os.Getenv(key); v != "" {
		return v
	}
	return def
}
