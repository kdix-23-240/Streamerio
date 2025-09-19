package counter

// Counter abstracts event counts & viewer activity tracking.
// Methods MUST be concurrency-safe.
type Counter interface {
	Increment(roomID, eventType string) (int64, error)
	Get(roomID, eventType string) (int64, error)
	Reset(roomID, eventType string) error
	UpdateViewerActivity(roomID, viewerID string) error
	GetActiveViewerCount(roomID string) (int64, error)
}
