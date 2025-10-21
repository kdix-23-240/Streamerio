package model

import "time"

type Room struct {
    ID         string     `json:"id" db:"id"`
    StreamerID string     `json:"streamer_id" db:"streamer_id"`
    CreatedAt  time.Time  `json:"created_at" db:"created_at"`
    ExpiresAt  *time.Time `json:"expires_at" db:"expires_at"`
    Status     string     `json:"status" db:"status"`
    Settings   string     `json:"settings" db:"settings"`
    EndedAt    *time.Time `json:"ended_at" db:"ended_at"`
}