package logger

import (
    "fmt"
    "io"
    "log/slog"
    "os"
    "strings"
)

type Config struct {
    Level     string
    Format    string
    AddSource bool
    Output    io.Writer
}

func Init(cfg Config) (*slog.Logger, error) {
    handler, err := buildHandler(cfg)
    if err != nil {
        return nil, err
    }

    logger := slog.New(handler)
    slog.SetDefault(logger)

    return logger, nil
}

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

    opts := &slog.HandlerOptions{
        Level:     lvlVar,
        AddSource: cfg.AddSource,
    }

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
