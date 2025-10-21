package counter

// Counter: イベントカウントと視聴者アクティビティ管理
type Counter interface {
    Increment(roomID, eventType string) (int64, error)
    Get(roomID, eventType string) (int64, error)
    Reset(roomID, eventType string) error
    UpdateViewerActivity(roomID, viewerID string) error
    GetActiveViewerCount(roomID string) (int64, error)
}