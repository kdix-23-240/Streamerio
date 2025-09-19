-- ENUM型の定義
CREATE TYPE room_status AS ENUM('active', 'inactive', 'expired');
CREATE TYPE event_type_enum AS ENUM('button1', 'button2', 'button3', 'button4', 'button5', 'button6');

-- ルームテーブル
CREATE TABLE rooms (
    id VARCHAR(36) PRIMARY KEY,
    streamer_id VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP,
    status room_status DEFAULT 'active',
    settings JSONB
);

-- イベントテーブル
CREATE TABLE events (
    id BIGSERIAL PRIMARY KEY,
    room_id VARCHAR(36) NOT NULL,
    viewer_id VARCHAR(255),
    event_type event_type_enum NOT NULL,
    triggered_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    metadata JSONB,
    FOREIGN KEY (room_id) REFERENCES rooms(id)
);

-- ゲームイベントテーブル
CREATE TABLE game_events (
    id BIGSERIAL PRIMARY KEY,
    room_id VARCHAR(36) NOT NULL,
    event_type event_type_enum NOT NULL,
    trigger_count INT NOT NULL,
    sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (room_id) REFERENCES rooms(id)
);