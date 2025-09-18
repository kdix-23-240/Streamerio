using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;

public class ConnectionEstablishment : MonoBehaviour
{
    private WebSocket websocket;

    public async void StartConnectionEstablishment()
    {
        // websocket = new WebSocket("ws://echo.websocket.org");
        websocket = new WebSocket("ws://localhost:8888/ws");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            // Reading a plain text message
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Received OnMessage! (" + bytes.Length + " bytes) " + message);
        };

        await websocket.Connect();
    }
}
