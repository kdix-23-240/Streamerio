using System;
using Common;
using UnityEngine;
using NativeWebSocket;

public class Connection : SingletonBase<Connection>
{
  private bool isConnected = false;
  WebSocket websocket;

  [SerializeField]
  public static string id;

  private void Update()
  {
    if (isConnected)
    {
    #if !UNITY_WEBGL || UNITY_EDITOR
      websocket.DispatchMessageQueue();
    #endif
    }
  }

  // websocketのコネクションを確立する
  public void connectWebSocket()
  {
    if (isConnected)
    {
      Debug.Log("WebSocket is already connected!");
      return;
    }
    
    // WebSocketのインスタンスを生成
    websocket = new WebSocket("wss://streamerio-282618030957.asia-northeast1.run.app/ws-unity");

    if (websocket == null)
    {
      Debug.Log("WebSocket is null!");
      return;
    }
    else{
      Debug.Log("WebSocket is ok!");
    }

    // WebSocketのイベントを焼き付け
    websocket.OnOpen += () =>
    {
      Debug.Log("Connection open!");
      isConnected = true;
    };

    websocket.OnError += (e) =>
    {
      Debug.Log("Error! " + e);
    };

    websocket.OnClose += (e) =>
    {
      Debug.Log("Connection closed!");
      isConnected = false;
    };

    websocket.OnMessage += (bytes) => reciveWebSocketMessage(bytes);

    websocket.Connect();
    return;
  }

  // WebSocketからメッセージを受信する
  public void reciveWebSocketMessage(byte[] bytes)
  {
    var message = System.Text.Encoding.UTF8.GetString(bytes);
    Debug.Log($"Received: {message}");

    BaseMessage baseMessage = null;
    try
    {
      baseMessage = JsonUtility.FromJson<BaseMessage>(message);
    }
    catch (Exception ex)
    {
      Debug.Log($"JSON base parse error: {ex.Message}");
    }

    if (baseMessage == null || string.IsNullOrEmpty(baseMessage.type))
    {
      Debug.Log("No type field in JSON message.");
      return;
    }

    if (baseMessage.type == "room_created")
    {
      try
      {
        var room = JsonUtility.FromJson<RoomCreatedNotification>(message);
        if (room != null)
        {
          id = room.room_id;
          return;
        }
      }
      catch (Exception ex)
      {
        Debug.Log($"room_created parse error: {ex.Message}");
      }
      Debug.Log("Failed to parse room_created message.");
      return;
    }

    if (baseMessage.type == "game_event")
    {
      try
      {
        var gameEvent = JsonUtility.FromJson<GameEventNotification>(message);
        if (gameEvent != null)
        {
          if (gameEvent.event_type == "help")
          {
            // Helpアクションを実行
          }
          else if (gameEvent.event_type == "attack")
          {
            // Attackアクションを実行
          }
          return;
        }
      }
      catch (Exception ex)
      {
        Debug.Log($"game_event parse error: {ex.Message}");
      }
      Debug.Log("Failed to parse game_event message.");
      return;
    }

    Debug.Log($"Unhandled JSON payload type: {baseMessage.type}");
    return;
  }

  // WebSocketを切断する
  public async void disconnectWebSocket()
  {
    if (!isConnected)
    {
      Debug.Log("WebSocket is not connected!");
      return;
    }

    if (websocket.State == WebSocketState.Closed)
    {
      Debug.Log("WebSocket is already closed!");
      return;
    }

    websocket.Close();
  }

  // UnityからWebSocketにメッセージを送信する
  // 使わないかも
  public async void SendWebSocketMessage(string message)
  {
    if (websocket.State == WebSocketState.Closed)
    {
      Debug.Log("WebSocket is not connected!");
      return;
    }

      websocket.SendText(message);
  }


  // アプリケーションが終了したときにwebsocketを閉じる
  private async void OnApplicationQuit()
  {
    await websocket.Close();
  }
  
  // JSONの型定義
  private class BaseMessage
  {
    public string type;
  }

  private class RoomCreatedNotification
  {
    public string type;
    public string room_id;
    public string qr_code;
    public string web_url;
  }

  private class GameEventNotification
  {
    public string type;
    public string event_type;
    public int trigger_count;
  }
}
