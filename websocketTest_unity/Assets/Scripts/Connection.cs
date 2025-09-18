using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using NativeWebSocket;
using System.Threading.Tasks;

public class Connection : MonoBehaviour
{
  WebSocket websocket;

  [SerializeField]
  private MockBuddyActions mockBuddyActions;

  [SerializeField]
  private TMP_Text logText;

  private async void Start()
  {
    Debug.Log("Start");
    connectWebSocket();
    Debug.Log("Start end");
  }

  private void Update()
  {
    #if !UNITY_WEBGL || UNITY_EDITOR
      websocket.DispatchMessageQueue();
    #endif
  }

  public void connectWebSocket()
  {
    // 既に接続されている場合は何もしない
    // if (websocket.State == WebSocketState.Open)
    // {
    //   Debug.Log("WebSocket is already connected!");
    //   return;
    // }

    websocket = new WebSocket("wss://5dc66f8872d7.ngrok-free.app/ws-unity");

    if (websocket == null)
    {
      Debug.Log("WebSocket is null!");
      return;
    }
    else{
      Debug.Log("WebSocket is ok!");
    }

    websocket.OnOpen += () =>
    {
      Debug.Log("Connection open!");
      logText.text = "Connection open!";
    };

    websocket.OnError += (e) =>
    {
      Debug.Log("Error! " + e);
      logText.text = "Error! " + e;
    };

    websocket.OnClose += (e) =>
    {
      Debug.Log("Connection closed!");
      logText.text = "Connection closed!";
    };

    websocket.OnMessage += (bytes) => reciveWebSocketMessage(bytes);

    websocket.Connect();
    return;
  }

  public void reciveWebSocketMessage(byte[] bytes)
  {
    var message = System.Text.Encoding.UTF8.GetString(bytes);
    Debug.Log("Received OnMessage! (" + message.Length + " bytes) " + message);
    
    switch (message)
    {
      case "1":
        mockBuddyActions.Attack();
        break;
      case "2":
        mockBuddyActions.Defend();
        break;
    }
  }

  public async void disconnectWebSocket()
  {
    if (websocket.State == WebSocketState.Closed)
    {
      Debug.Log("WebSocket is already closed!");
      return;
    }

    websocket.Close();
    logText.text = "Connection closed!";
  }

  public async void SendWebSocketMessage(string message)
  {
    if (websocket.State == WebSocketState.Closed)
    {
      Debug.Log("WebSocket is not connected!");
      return;
    }

      // Sending plain text
      websocket.SendText(message);
    
  }

  private async void OnApplicationQuit()
  {
    await websocket.Close();
  }
}
