package logger

import (
	"fmt"
	"io"
	"log/slog"
	"os"
	"strings"
)

// Config: ロガー初期化用設定
// Level は "debug"/"info"/"warn"/"error"、Format は "text" または "json" を想定。
type Config struct {
	Level     string    // ログレベル (デフォルト: info)
	Format    string    // 出力フォーマット (text/json)
	AddSource bool      // 呼び出し元情報を付与するか
	Output    io.Writer // 出力先 (nil の場合は os.Stdout)
}

// Init: Config に基づいて slog ロガーを初期化し、デフォルトロガーとして登録する。
func Init(cfg Config) (*slog.Logger, error) {
	handler, err := buildHandler(cfg)
	if err != nil {
		return nil, err
	}
	logger := slog.New(handler)
	slog.SetDefault(logger)
	return logger, nil
}

// L: デフォルトロガーを返すヘルパ。
func L() *slog.Logger {
	return slog.Default()
}

func buildHandler(cfg Config) (slog.Handler, error) {
	level, err := parseLevel(cfg.Level)
	if err != nil {
		return nil, err
	}
	lvlVar := new(slog.LevelVar)
	lvlVar.Set(level)

	opts := &slog.HandlerOptions{Level: lvlVar, AddSource: cfg.AddSource}

	writer := cfg.Output
	if writer == nil {
		writer = os.Stdout
	}

	switch strings.ToLower(cfg.Format) {
	case "", "text", "plain", "console":
		return slog.NewTextHandler(writer, opts), nil
	case "json", "structured":
		return slog.NewJSONHandler(writer, opts), nil
	default:
		return nil, fmt.Errorf("unsupported log format: %s", cfg.Format)
	}
}

func parseLevel(level string) (slog.Level, error) {
	switch strings.ToLower(level) {
	case "", "info":
		return slog.LevelInfo, nil
	case "debug":
		return slog.LevelDebug, nil
	case "warn", "warning":
		return slog.LevelWarn, nil
	case "error", "err":
		return slog.LevelError, nil
	default:
		return slog.LevelInfo, fmt.Errorf("unsupported log level: %s", level)
	}
}
