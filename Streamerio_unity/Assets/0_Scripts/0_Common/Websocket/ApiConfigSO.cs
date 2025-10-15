using UnityEngine;

[CreateAssetMenu(fileName = "ApiConfigSO", menuName = "SO/Common/ApiConfigSO")]
public class ApiConfigSO : ScriptableObject
{
    [Header("フロントのURLフォーマット")]
    public string frontendUrlFormat = "https://streamerio.vercel.app/?streamer_id={0}";
    
    [Header("バックエンドのWebsocket用URL")]
    public string backendUrl = "wss://streamario-web-backend-282618030957.asia-northeast1.run.app/ws-unity";
    
    [Header("バックエンドのHTTP用URL")]
    public string backendHttpUrl = "https://streamario-web-backend-282618030957.asia-northeast1.run.app/";
}