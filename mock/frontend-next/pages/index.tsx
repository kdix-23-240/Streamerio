import { useState } from 'react'

export default function Home() {
  const [status, setStatus] = useState<string>("")
  const [loading, setLoading] = useState(false)

  const sendCommand = async () => {
    setLoading(true)
    setStatus("")
    try {
      const res = await fetch('http://localhost:8080/trigger', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message: 'Nextからの命令' })
      })
      const data = await res.json()
      setStatus(`送信完了: ${data.broadcast}`)
    } catch (e: any) {
      setStatus(`エラー: ${e?.message ?? 'unknown'}`)
    } finally {
      setLoading(false)
    }
  }

  return (
    <main style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', flexDirection: 'column', gap: 16 }}>
      <h1>シンプルコマンド送信</h1>
      <button onClick={sendCommand} disabled={loading} style={{ padding: '12px 20px', fontSize: 16 }}>
        {loading ? '送信中...' : '命令を送る'}
      </button>
      {status && <p>{status}</p>}
      <p style={{marginTop: 24, opacity: 0.7}}>別ページ(simple-client)を開いてWebSocket受信を確認してください。</p>
    </main>
  )
}

