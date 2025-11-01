package model

import "time"

type Viewer struct {
    ID        string     `json:"id" db:"id"`
    Name      *string    `json:"name" db:"name"`
    CreatedAt time.Time  `json:"created_at" db:"created_at"`
    UpdatedAt *time.Time `json:"updated_at" db:"updated_at"`
}