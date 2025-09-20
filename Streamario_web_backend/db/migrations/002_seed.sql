-- 002_seed.sql (idempotent seed)

-- 1. ENUM 値を不足分だけ追加
DO $$
BEGIN
    -- 既存ならスキップ (IF NOT EXISTS が使えない古い PG 対応で EXCEPTION を握りつぶす書き方)
    BEGIN
        ALTER TYPE event_type_enum ADD VALUE IF NOT EXISTS 'help_speed';
    EXCEPTION WHEN duplicate_object THEN NULL; END;

    BEGIN
        ALTER TYPE event_type_enum ADD VALUE IF NOT EXISTS 'help_jump';
    EXCEPTION WHEN duplicate_object THEN NULL; END;

    BEGIN
        ALTER TYPE event_type_enum ADD VALUE IF NOT EXISTS 'help_heal';
    EXCEPTION WHEN duplicate_object THEN NULL; END;

    BEGIN
        ALTER TYPE event_type_enum ADD VALUE IF NOT EXISTS 'hinder_slow';
    EXCEPTION WHEN duplicate_object THEN NULL; END;

    BEGIN
        ALTER TYPE event_type_enum ADD VALUE IF NOT EXISTS 'hinder_slip';
    EXCEPTION WHEN duplicate_object THEN NULL; END;

    BEGIN
        ALTER TYPE event_type_enum ADD VALUE IF NOT EXISTS 'hinder_damage';
    EXCEPTION WHEN duplicate_object THEN NULL; END;
END
$$;

-- 2. サンプルルーム
INSERT INTO rooms (id, streamer_id, created_at, status, settings)
SELECT 'demo-room-1', 'streamer_demo', NOW(), 'active', '{}'::jsonb
WHERE NOT EXISTS (SELECT 1 FROM rooms WHERE id='demo-room-1');

-- 3. サンプルイベント1件
INSERT INTO events (room_id, viewer_id, event_type, metadata)
SELECT 'demo-room-1', 'viewerA', 'help_speed', '{}'::jsonb
WHERE NOT EXISTS (SELECT 1 FROM events WHERE room_id='demo-room-1' AND event_type='help_speed');

-- 4. サンプル game_events
INSERT INTO game_events (room_id, event_type, trigger_count)
SELECT 'demo-room-1', 'help_speed', 5
WHERE NOT EXISTS (SELECT 1 FROM game_events WHERE room_id='demo-room-1' AND event_type='help_speed');