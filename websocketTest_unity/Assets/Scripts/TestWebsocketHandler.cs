using UnityEngine;

public class TestWebsocketHandler : MonoBehaviour
{
    [SerializeField]
    private Connection connection;

    public void connectWebSocket()
    {
        connection.connectWebSocket();
    }

    public void disconnectWebSocket()
    {
        connection.disconnectWebSocket();
    }
}
