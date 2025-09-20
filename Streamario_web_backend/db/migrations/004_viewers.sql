-- 004_viewers.sql : viewer ID 管理テーブルの追加

CREATE TABLE IF NOT EXISTS viewers (
    id VARCHAR(36) PRIMARY KEY,
    name TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP
);
