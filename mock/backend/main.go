package main

// このファイルは外部ライブラリに依存しない最小限のWebSocketサーバ実装です。
// 役割:
// - /trigger: POSTを受け取って、接続中の全WebSocketクライアントへテキストメッセージをブロードキャスト
// - /ws: 簡易WebSocketエンドポイント（ハンドシェイク～フレーム送受信）

import (
    "bufio"         // ストリーム読み取りのバッファリング
    "crypto/sha1"   // WebSocketハンドシェイクのAccept-Key生成に使用
    "encoding/base64"
    "encoding/binary" // WebSocketフレームの長さフィールドの変換に使用
    "encoding/json"
    "fmt"
    "io"
    "log"
    "net"
    "net/http"
    "strings"
    "sync"
)

// 簡易WebSocketハブ（外部ライブラリ非依存）
// Hub は接続中のWebSocketクライアント(net.Conn)を管理し、
// 全員にまとめてメッセージを送るための簡易ハブです。
type Hub struct {
    mu    sync.Mutex
    conns map[net.Conn]struct{}
}

func NewHub() *Hub {
    return &Hub{conns: make(map[net.Conn]struct{})}
}

// Add は新しい接続をハブへ登録します。
func (h *Hub) Add(c net.Conn) {
    h.mu.Lock()
    h.conns[c] = struct{}{}
    h.mu.Unlock()
}

// Remove は接続をハブから削除し、ソケットを閉じます。
func (h *Hub) Remove(c net.Conn) {
    h.mu.Lock()
    if _, ok := h.conns[c]; ok {
        delete(h.conns, c)
        _ = c.Close()
    }
    h.mu.Unlock()
}

// Broadcast は登録されている全接続へテキストメッセージを送信します。
// 送信エラーが起きた接続は切断されたものとして削除します。
func (h *Hub) Broadcast(msg string) {
    h.mu.Lock()
    for c := range h.conns {
        if err := writeTextFrame(c, []byte(msg)); err != nil {
            // 書き込み失敗は切断とみなして掃除
            delete(h.conns, c)
            _ = c.Close()
        }
    }
    h.mu.Unlock()
}

var hub = NewHub()

// /trigger へのPOSTで全WSクライアントに通知
func triggerHandler(w http.ResponseWriter, r *http.Request) {
    // 簡易CORS対応。Next(別ポート)からの呼び出しを許可しています。
    addCORS(w)
    if r.Method == http.MethodOptions {
        w.WriteHeader(http.StatusNoContent)
        return
    }
    if r.Method != http.MethodPost {
        http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
        return
    }

    // JSON本文から message を取り出し、空ならデフォルト文言にします。
    var body struct{
        Message string `json:"message"`
    }
    if err := json.NewDecoder(r.Body).Decode(&body); err != nil && err != io.EOF {
        http.Error(w, "invalid json", http.StatusBadRequest)
        return
    }
    msg := body.Message
    if strings.TrimSpace(msg) == "" {
        msg = "命令が来たよ"
    } else {
        msg = fmt.Sprintf("命令が来たよ: %s", msg)
    }

    hub.Broadcast(msg)

    w.Header().Set("Content-Type", "application/json")
    _ = json.NewEncoder(w).Encode(map[string]string{
        "status": "ok",
        "broadcast": msg,
    })
}

// 簡易WebSocketエンドポイント（/ws）
func wsHandler(w http.ResponseWriter, r *http.Request) {
    // WebSocketアップグレード条件をゆるくチェック
    if strings.ToLower(r.Header.Get("Connection")) != "upgrade" || strings.ToLower(r.Header.Get("Upgrade")) != "websocket" {
        http.Error(w, "upgrade required", http.StatusBadRequest)
        return
    }
    key := r.Header.Get("Sec-WebSocket-Key")
    if key == "" {
        http.Error(w, "bad handshake", http.StatusBadRequest)
        return
    }

    // HTTPコネクションをハイジャックし、生のTCPソケットへ切り替えます。
    hj, ok := w.(http.Hijacker)
    if !ok {
        http.Error(w, "hijacking not supported", http.StatusInternalServerError)
        return
    }
    conn, rw, err := hj.Hijack()
    if err != nil {
        log.Printf("hijack error: %v", err)
        return
    }

    // WebSocketハンドシェイク応答の書き込み。
    // Sec-WebSocket-Accept は `base64( SHA1(key + GUID) )` で計算します。
    accept := computeAcceptKey(key)
    _, _ = rw.WriteString("HTTP/1.1 101 Switching Protocols\r\n")
    _, _ = rw.WriteString("Upgrade: websocket\r\n")
    _, _ = rw.WriteString("Connection: Upgrade\r\n")
    _, _ = rw.WriteString("Sec-WebSocket-Accept: " + accept + "\r\n")
    _, _ = rw.WriteString("\r\n")
    if err := rw.Flush(); err != nil {
        _ = conn.Close()
        return
    }

    hub.Add(conn)
    // クライアントからのフレームを読み、切断やPingに対応します。
    go readLoop(conn)
}

func main() {
    mux := http.NewServeMux()
    mux.HandleFunc("/trigger", triggerHandler)
    mux.HandleFunc("/ws", wsHandler)
    mux.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
        addCORS(w)
        _, _ = w.Write([]byte("OK"))
    })

    addr := ":8080"
    log.Printf("Go backend listening on %s", addr)
    if err := http.ListenAndServe(addr, mux); err != nil {
        log.Fatal(err)
    }
}

// --- WebSocketユーティリティ ---

// computeAcceptKey は WebSocket の Accept-Key を計算します。
// 仕様: accept = base64( SHA1( clientKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" ) )
func computeAcceptKey(key string) string {
    const wsGUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
    h := sha1.Sum([]byte(key + wsGUID))
    return base64.StdEncoding.EncodeToString(h[:])
}

func readLoop(conn net.Conn) {
    // クライアントから送られるフレームを読み取り、
    // Closeフレームの検知やPing/Pongへの応答を行います。
    // WebSocketの仕様上、クライアント→サーバのフレームはマスク必須です。
    br := bufio.NewReader(conn)
    for {
        // 最低2バイトのヘッダ
        header := make([]byte, 2)
        if _, err := io.ReadFull(br, header); err != nil {
            hub.Remove(conn)
            return
        }
        fin := (header[0] & 0x80) != 0
        _ = fin
        opcode := header[0] & 0x0F
        masked := (header[1] & 0x80) != 0
        payloadLen := int(header[1] & 0x7F)

        if payloadLen == 126 {
            ext := make([]byte, 2)
            if _, err := io.ReadFull(br, ext); err != nil {
                hub.Remove(conn)
                return
            }
            payloadLen = int(binary.BigEndian.Uint16(ext))
        } else if payloadLen == 127 {
            ext := make([]byte, 8)
            if _, err := io.ReadFull(br, ext); err != nil {
                hub.Remove(conn)
                return
            }
            // 長すぎる場合は切る（デモ用途）
            l := binary.BigEndian.Uint64(ext)
            if l > 1<<31 {
                hub.Remove(conn)
                return
            }
            payloadLen = int(l)
        }

        var maskKey [4]byte
        if masked {
            if _, err := io.ReadFull(br, maskKey[:]); err != nil {
                hub.Remove(conn)
                return
            }
        }

        payload := make([]byte, payloadLen)
        if _, err := io.ReadFull(br, payload); err != nil {
            hub.Remove(conn)
            return
        }
        if masked {
            for i := 0; i < payloadLen; i++ {
                payload[i] ^= maskKey[i%4]
            }
        }

        switch opcode {
        case 0x1: // text
            // クライアントからのテキストはこのデモでは未使用
        case 0x8: // close
            hub.Remove(conn)
            return
        case 0x9: // ping -> pong返す
            _ = writePong(conn, payload)
        default:
            // nop (binaryや続きフレームなど)
        }
        _ = fin
    }
}

// writeTextFrame はサーバ→クライアントのテキストフレームを生成して送ります。
// サーバ送信のフレームはマスク不要です。
func writeTextFrame(w io.Writer, payload []byte) error {
    // 0x81 は FIN(1) + Text(opcode=0x1)
    if _, err := w.Write([]byte{0x81}); err != nil {
        return err
    }
    l := len(payload)
    switch {
    case l <= 125:
        if _, err := w.Write([]byte{byte(l)}); err != nil {
            return err
        }
    case l <= 65535:
        if _, err := w.Write([]byte{126}); err != nil {
            return err
        }
        var b [2]byte
        binary.BigEndian.PutUint16(b[:], uint16(l))
        if _, err := w.Write(b[:]); err != nil {
            return err
        }
    default:
        if _, err := w.Write([]byte{127}); err != nil {
            return err
        }
        var b [8]byte
        binary.BigEndian.PutUint64(b[:], uint64(l))
        if _, err := w.Write(b[:]); err != nil {
            return err
        }
    }
    _, err := w.Write(payload)
    return err
}

// writePong はPingに対するPongフレームを返します。
func writePong(w io.Writer, payload []byte) error {
    if _, err := w.Write([]byte{0x8A}); err != nil { // 0x8A は FIN + Pong(opcode=0xA)
        return err
    }
    l := len(payload)
    switch {
    case l <= 125:
        if _, err := w.Write([]byte{byte(l)}); err != nil {
            return err
        }
    case l <= 65535:
        if _, err := w.Write([]byte{126}); err != nil {
            return err
        }
        var b [2]byte
        binary.BigEndian.PutUint16(b[:], uint16(l))
        if _, err := w.Write(b[:]); err != nil {
            return err
        }
    default:
        if _, err := w.Write([]byte{127}); err != nil {
            return err
        }
        var b [8]byte
        binary.BigEndian.PutUint64(b[:], uint64(l))
        if _, err := w.Write(b[:]); err != nil {
            return err
        }
    }
    _, err := w.Write(payload)
    return err
}

// addCORS はデモ用途の緩いCORSヘッダを付与します。
// 本番用途ではオリジンを限定してください。
func addCORS(w http.ResponseWriter) {
    w.Header().Set("Access-Control-Allow-Origin", "*")
    w.Header().Set("Access-Control-Allow-Methods", "POST, OPTIONS")
    w.Header().Set("Access-Control-Allow-Headers", "Content-Type")
}
