package config

import (
	"fmt"
	"os"
)

// Config アプリ全体の設定値
type Config struct {
	Port        string
	FrontendURL string
	DatabaseURL string
	RedisURL    string // host:port 形式 (例: localhost:6379)
}

// Load 環境変数から設定を読み込む。欠けている値には安全なデフォルトを適用。
// 優先順位:
// 1. 直接的な URL (DATABASE_URL, REDIS_URL)
// 2. 個別値の合成 (DB_HOST など)
// 3. デフォルト
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
	if v := os.Getenv(key); v != "" { return v }
	return def
}

