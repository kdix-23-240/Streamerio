-- 003_game_end.sql : ゲーム終了処理向けのスキーマ拡張

-- room_status に ended を追加（既に存在する場合は何もしない）
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_enum e
        JOIN pg_type t ON e.enumtypid = t.oid
        WHERE t.typname = 'room_status'
          AND e.enumlabel = 'ended'
    ) THEN
        ALTER TYPE room_status ADD VALUE 'ended';
    END IF;
END$$;

-- ルーム終了時刻を記録
ALTER TABLE rooms ADD COLUMN IF NOT EXISTS ended_at TIMESTAMP;

