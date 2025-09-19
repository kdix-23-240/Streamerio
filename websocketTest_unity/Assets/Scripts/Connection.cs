using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using NativeWebSocket;
using System.Threading.Tasks;

  // JSONの型定義
  class BaseMessage
  {
    public string type;
  }

  class RoomCreatedNotification
  {
    public string type;
    public string room_id;
    public string qr_code;
    public string web_url;
  }

  class GameEventNotification
  {
    public string type;
    public string event_type;
    public int trigger_count;
  }

public class Connection : MonoBehaviour
{
  private bool isConnected = false;
  WebSocket websocket;

  [SerializeField]
  private MockBuddyActions mockBuddyActions;

  [SerializeField]
  public static string id;

  private async void Start()
  {
    // connectWebSocket();
  }

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

    // TODO: 本番環境のURLに変更する
    websocket = new WebSocket("wss://5dc66f8872d7.ngrok-free.app/ws-unity");

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
            mockBuddyActions.Defend();
          }
          else if (gameEvent.event_type == "attack")
          {
            mockBuddyActions.Attack();
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
}
