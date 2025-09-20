-- 002_seed.sql (idempotent seed)

-- 1. ENUM 値を不足分だけ追加
DO $$
BEGIN
    -- 既存ならスキップ (IF NOT EXISTS が使えない古い PG 対応で EXCEPTION を握りつぶす書き方)
    BEGIN
        ALTER TYPE event_type_enum ADD VALUE IF NOT EXISTS 'skill1';
    EXCEPTION WHEN duplicate_object THEN NULL; END;

    BEGIN
        ALTER TYPE event_type_enum ADD VALUE IF NOT EXISTS 'skill2';
    EXCEPTION WHEN duplicate_object THEN NULL; END;

    BEGIN
        ALTER TYPE event_type_enum ADD VALUE IF NOT EXISTS 'skill3';
    EXCEPTION WHEN duplicate_object THEN NULL; END;

    BEGIN
        ALTER TYPE event_type_enum ADD VALUE IF NOT EXISTS 'enemy1';
    EXCEPTION WHEN duplicate_object THEN NULL; END;

    BEGIN
        ALTER TYPE event_type_enum ADD VALUE IF NOT EXISTS 'enemy2';
    EXCEPTION WHEN duplicate_object THEN NULL; END;

    BEGIN
        ALTER TYPE event_type_enum ADD VALUE IF NOT EXISTS 'enemy3';
    EXCEPTION WHEN duplicate_object THEN NULL; END;
END
$$;

-- 2. サンプルルーム
INSERT INTO rooms (id, streamer_id, created_at, status, settings)
SELECT 'demo-room-1', 'streamer_demo', NOW(), 'active', '{}'::jsonb
WHERE NOT EXISTS (SELECT 1 FROM rooms WHERE id='demo-room-1');

-- 3. サンプルイベント1件
INSERT INTO events (room_id, viewer_id, event_type, metadata)
SELECT 'demo-room-1', 'viewerA', 'skill1', '{}'::jsonb
WHERE NOT EXISTS (SELECT 1 FROM events WHERE room_id='demo-room-1' AND event_type='skill1');

-- 4. サンプル game_events
INSERT INTO game_events (room_id, event_type, trigger_count)
SELECT 'demo-room-1', 'skill2', 5
WHERE NOT EXISTS (SELECT 1 FROM game_events WHERE room_id='demo-room-1' AND event_type='skill2');