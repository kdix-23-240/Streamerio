using UnityEngine;

public class sendMessageOnWebSocket : MonoBehaviour
{
    [SerializeField]
    private Connection connection;

    public string message;

    public void SendMessageOnWebSocket()
    {
        connection.SendWebSocketMessage(message);
    }
}
