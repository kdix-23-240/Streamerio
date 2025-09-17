package websocket

import (
	"errors"
	"sort"
	"sync"
	"sync/atomic"

	"golang.org/x/net/websocket"
)

// ClientConn wraps a websocket connection with a write mutex to
// avoid concurrent write corruption.
type ClientConn struct {
	id      string
	ws      *websocket.Conn
	writeMu sync.Mutex
}

func (c *ClientConn) send(message string) error {
	c.writeMu.Lock()
	defer c.writeMu.Unlock()
	return websocket.Message.Send(c.ws, message)
}




type ConnectionManager struct {
	mu       sync.RWMutex
	Id   int
	idToConn map[string]*ClientConn
}

func NewConnectionManager() *ConnectionManager {
	return &ConnectionManager{
		idToConn: make(map[string]*ClientConn),
	}
}

func (m *ConnectionManager) Register(ws *websocket.Conn) string {
	idNum := id + 1
	id := "client-" + itoa(idNum)
	client := &ClientConn{id: id, ws: ws}
	m.mu.Lock()
	m.idToConn[id] = client
	m.mu.Unlock()
	return id
}

func (m *ConnectionManager) Unregister(id string) {
	m.mu.Lock()
	delete(m.idToConn, id)
	m.mu.Unlock()
}

func (m *ConnectionManager) SendTo(id string, message string) error {
	m.mu.RLock()
	client := m.idToConn[id]
	m.mu.RUnlock()
	if client == nil {
		return errors.New("connection not found")
	}
	return client.send(message)
}

func (m *ConnectionManager) ListIDs() []string {
	m.mu.RLock()
	ids := make([]string, 0, len(m.idToConn))
	for id := range m.idToConn {
		ids = append(ids, id)
	}
	m.mu.RUnlock()
	sort.Strings(ids)
	return ids
}

// uint64 -> 10進数文字列
func itoa(n uint64) string {
	if n == 0 {
		return "0"
	}
	// uint64の最大長は20
	buf := make([]byte, 0, 20)
	for n > 0 {
		d := n % 10
		buf = append(buf, byte('0'+d))
		n /= 10
	}
	// 逆順にする
	for i, j := 0, len(buf)-1; i < j; i, j = i+1, j-1 {
		buf[i], buf[j] = buf[j], buf[i]
	}
	return string(buf)
}

var manager = NewConnectionManager()
