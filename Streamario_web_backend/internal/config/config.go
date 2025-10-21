package config

import (
    "fmt"
    "os"
)

// Config: アプリケーション全体の設定
type Config struct {
    Port        string
    FrontendURL string
    DatabaseURL string
    RedisURL    string
}

// Load: 環境変数から設定をロード
func Load() (*Config, error) {
    cfg := &Config{}

    cfg.Port = getEnv("PORT", "8888")
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
        sslmode := getEnv("DB_SSLMODE", "disable")
        cfg.DatabaseURL = fmt.Sprintf(
            "host=%s port=%s user=%s password=%s dbname=%s sslmode=%s",
            host, port, user, pass, name, sslmode,
        )
    }

    // Redis URL
    if rurl := os.Getenv("REDIS_URL"); rurl != "" {
        cfg.RedisURL = rurl
    } else if addr := os.Getenv("REDIS_ADDR"); addr != "" {
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
