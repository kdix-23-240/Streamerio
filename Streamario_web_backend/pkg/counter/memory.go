package counter

import (
	"sync"
	"time"
)

type memoryCounter struct {
	mu      sync.RWMutex
	counts  map[string]map[string]int64 // roomID -> eventType -> count
	viewers map[string]map[string]int64 // roomID -> viewerID -> lastUnix
	window  time.Duration
}

func NewMemoryCounter() Counter {
	return &memoryCounter{
		counts:  make(map[string]map[string]int64),
		viewers: make(map[string]map[string]int64),
		window:  5 * time.Minute,
	}
}

func (m *memoryCounter) Increment(roomID, eventType string) (int64, error) {
	m.mu.Lock(); defer m.mu.Unlock()
	if _, ok := m.counts[roomID]; !ok { m.counts[roomID] = make(map[string]int64) }
	m.counts[roomID][eventType]++
	return m.counts[roomID][eventType], nil
}

func (m *memoryCounter) Get(roomID, eventType string) (int64, error) {
	m.mu.RLock(); defer m.mu.RUnlock()
	if evMap, ok := m.counts[roomID]; ok { return evMap[eventType], nil }
	return 0, nil
}

func (m *memoryCounter) Reset(roomID, eventType string) error {
	m.mu.Lock(); defer m.mu.Unlock()
	if evMap, ok := m.counts[roomID]; ok { evMap[eventType] = 0 }
	return nil
}

func (m *memoryCounter) UpdateViewerActivity(roomID, viewerID string) error {
	m.mu.Lock(); defer m.mu.Unlock()
	if _, ok := m.viewers[roomID]; !ok { m.viewers[roomID] = make(map[string]int64) }
	m.viewers[roomID][viewerID] = time.Now().Unix()
	return nil
}

func (m *memoryCounter) GetActiveViewerCount(roomID string) (int64, error) {
	cutoff := time.Now().Add(-m.window).Unix()
	m.mu.Lock(); defer m.mu.Unlock()
	vMap, ok := m.viewers[roomID]
	if !ok { return 0, nil }
	var c int64
	for id, ts := range vMap {
		if ts >= cutoff { c++ } else { delete(vMap, id) }
	}
	return c, nil
}
