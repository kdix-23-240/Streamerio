using System;
using System.Collections.Generic;
using System.Globalization;
using Common;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using NativeWebSocket;
using R3;
using UnityEngine.Networking;

public class WebSocketManager : SingletonBase<WebSocketManager>, IWebSocketManager, IDisposable
{
  private WebSocket _websocket;

  private ReactiveProperty<bool> _isConnectedProp = new ReactiveProperty<bool>(false);
  public ReadOnlyReactiveProperty<bool> IsConnectedProp => _isConnectedProp;

  private string _roomId = string.Empty;

  private Dictionary<FrontKey, Subject<Unit>> _frontEventDict = new Dictionary<FrontKey, Subject<Unit>>();
  public IReadOnlyDictionary<FrontKey, Subject<Unit>> FrontEventDict => _frontEventDict;
  
  private GameEndSummaryNotification _gameEndSummary = null;
  public GameEndSummaryNotification GameEndSummary => _gameEndSummary;

  [SerializeField]
  private ApiConfigSO _apiConfigSO;
  
  private string _qrCodeURL = string.Empty;

  private string _frontendUrlFormat = null; 

  private string _backendHttpUrl = null;

  private string _backendWsBaseUrl = null;

  private string _frontendQueryParamFormat = null;
  
  private string _gameEndResponse = null;

  protected override void Awake()
  {
    base.Awake();
    
    if (_apiConfigSO != null)
    {
      _frontendUrlFormat = _apiConfigSO.frontendUrlFormat;
      _backendHttpUrl = _apiConfigSO.backendHttpUrl;
      _backendWsBaseUrl = _apiConfigSO.backendWsUrl;
      _frontendQueryParamFormat = _apiConfigSO.frontendQueryParamFormat;
      _gameEndResponse = _apiConfigSO.gameEndResponse;
    }
    else
    {
      Debug.LogError("ApiConfigSO is not assigned. Please assign an ApiConfigSO asset in the Inspector.");
    }
    
    foreach (FrontKey key in Enum.GetValues(typeof(FrontKey)))
    {
      _frontEventDict[key] = new Subject<Unit>();
    }
  }

  private void Update()
  {
    if (_isConnectedProp.Value)
    {
    #if !UNITY_WEBGL || UNITY_EDITOR
      _websocket.DispatchMessageQueue();
    #endif
    }
  }

  // websocketのコネクションを確立する
  public async UniTask ConnectWebSocketAsync([CanBeNull] string websocketId = null)
  {
    if (_isConnectedProp.Value)
    {
      Debug.Log("WebSocket is already connected!");
      return;
    }
    
    // WebSocketのインスタンスを生成
    string websocketUrl;
    if (string.IsNullOrEmpty(websocketId))
    {
      websocketUrl = _backendWsBaseUrl;
    }
    else
    {
      websocketUrl = ZString.Format(_frontendQueryParamFormat, _backendWsBaseUrl, websocketId);
    }
    
    _websocket = new WebSocket(websocketUrl);

    if (_websocket == null)
    {
      Debug.Log("WebSocket is null!");
      return;
    }
    else{
      Debug.Log("WebSocket is ok!");
    }

    // WebSocketのイベントを焼き付け
    _websocket.OnOpen += () =>
    {
      Debug.Log("Connection open!");
      _isConnectedProp.Value = true;
    };

    _websocket.OnError += (e) =>
    {
      Debug.LogError($"Error! {e}");
      _isConnectedProp.Value = false;
    };

    _websocket.OnClose += (e) =>
    {
      Debug.Log("Connection closed!");
      _isConnectedProp.Value = false;
    };

    _websocket.OnMessage += (bytes) => ReceiveWebSocketMessage(bytes);

    await _websocket.Connect();
    return;
  }

  ///<summary>
  /// JSON文字列を指定された型に安全にパースする
  /// </summary>
  /// <typeparam name="T">パース対象の型</typeparam>
  /// <param name="json">JSON文字列</param>
  /// <param name="data">パース結果（成功時）</param>
  /// <returns>パースが成功した場合true、失敗した場合false</returns>
  private bool TryJsonParse<T>(string json, out T data)
  {
    data = default(T);
    
    if (string.IsNullOrEmpty(json))
    {
      return false;
    }
    
    try
    {
      data = JsonUtility.FromJson<T>(json);
      return data != null;
    }
    catch (Exception ex)
    {
      Debug.LogError($"JSON parse error for type {typeof(T).Name}: {ex.Message}");
      return false;
    }
  }

  ///<summary>
  /// WebSocketからメッセージを受信する
  ///</summary>
  private void ReceiveWebSocketMessage(byte[] bytes)
  {
    var message = System.Text.Encoding.UTF8.GetString(bytes);
    Debug.Log($"Received: {message}");

    BaseMessage baseMessage = null;
    MessageType messageType = MessageType.unknown;
    
    if (!TryJsonParse(message, out baseMessage))
    {
      Debug.Log("No type field in JSON message.");
      return;
    }

    if (string.IsNullOrEmpty(baseMessage.type))
    {
      Debug.Log("No type field in JSON message.");
      return;
    }
    
    if (!Enum.TryParse<MessageType>(baseMessage.type, out messageType))
    {
      Debug.Log($"MessageType parse error: Unknown message type '{baseMessage.type}'");
      return;
    }

    switch (messageType)
    {
      case MessageType.room_created:
        if (TryJsonParse(message, out RoomCreatedNotification room))
        {
          _roomId = room.room_id;
        }
        else
        {
          Debug.Log("Failed to parse room_created message.");
        }
        break;
      case MessageType.game_event:
        if (TryJsonParse(message, out GameEventNotification gameEvent))
        {
          if (Enum.TryParse<FrontKey>(gameEvent.event_type, true, out var keyType))
          {
            _frontEventDict[keyType]?.OnNext(Unit.Default);
          }
          else
          {
            Debug.Log($"FrontKey parse error: Unknown event type '{gameEvent.event_type}'");
          }
        }
        else
        {
          Debug.Log("Failed to parse game_event message.");
        }
        break;

      case MessageType.game_end_summary:
        try
        {
          var root = MiniJSON.Json.Deserialize(message) as Dictionary<string, object>;
          if (root == null) throw new Exception("root is null");

          _gameEndSummary = new GameEndSummaryNotification();

          // --- team_tops の分解 ---
          if (root.TryGetValue("team_tops", out var teamObj) &&
              teamObj is Dictionary<string, object> teamDict)
          {
            foreach (var kv in teamDict)
            {
              if (kv.Value is Dictionary<string, object> val)
              {
                var detail = new GameEndSummaryNotification.SummaryDetail
                {
                  count       = val.TryGetValue("count", out var c)
                    ? Convert.ToInt32(c, CultureInfo.InvariantCulture)
                    : 0
                  ,
                  viewer_id   = val.TryGetValue("viewer_id", out var vid) ? vid?.ToString() : null,
                  viewer_name = val.TryGetValue("viewer_name", out var vname) ? vname?.ToString() : null,
                };
                _gameEndSummary.SummaryDetails[kv.Key] = detail; // "all" / "enemy" / "skill"
              }
            }
          }
          
          break;
        }
        catch (Exception ex)
        {
          Debug.Log($"game_end_summary parse error: {ex.Message}");
        }
        
        Debug.Log("Failed to parse game_event message.");
        break;
    }

    Debug.Log($"Unhandled JSON payload type: {messageType}");
    return;
  }

  ///<summary>
  /// WebSocketを切断する
  ///</summary>
  public async UniTask DisconnectWebSocketAsync()
  {
    if (!_isConnectedProp.Value)
    {
      Debug.Log("WebSocket is not connected!");
      return;
    }

    if (_websocket.State == WebSocketState.Closed)
    {
      Debug.Log("WebSocket is already closed!");
      return;
    }

    await _websocket.Close();
  }
  
  ///<summary>
  /// フロントエンドのURLを取得する
  ///</summary>
  public async UniTask<string> GetFrontUrlAsync()
  {
    if (_qrCodeURL != string.Empty)
    {
      return _qrCodeURL;
    }
    
    await UniTask.WaitWhile(() => _roomId == string.Empty);
    _qrCodeURL = ZString.Format(_frontendUrlFormat, _roomId);
    
    return _qrCodeURL;
  }

  ///<summary>
  /// ゲーム終了通知
  ///</summary>
  public async UniTask GameEndAsync()
  {
    await SendWebSocketMessageAsync( _gameEndResponse );
  }


  ///<summary>
  /// UnityからWebSocketにメッセージを送信する
  ///</summary>
  private async UniTask SendWebSocketMessageAsync(string message)
  {
    if (_websocket.State == WebSocketState.Closed)
    {
      Debug.Log("WebSocket is not connected!");
      return;
    }

    await _websocket.SendText(message);
  }

  ///<summary>
  /// ヘルスチェック
  ///</summary>
  public void HealthCheck()
  {
    UnityWebRequest.Get(_backendHttpUrl).SendWebRequest();
    Debug.Log("HealthCheck");
  }


  ///<summary>
  /// リソースを解放する
  ///</summary>
  public void Dispose()
  {
    try
    {
      DisconnectWebSocketAsync().Forget();
    }
    catch (Exception ex)
    {
      Debug.Log($"Error during WebSocket disconnection: {ex.Message}");
    }
  }

  ///<summary>
  /// アプリケーションが終了したときにwebsocketを閉じる
  ///</summary>
  private void OnApplicationQuit()
  {
    Dispose();
  }
  
  ///<summary>
  /// JSONの型定義
  ///</summary>
  private class BaseMessage
  {
    public string type;
  }

  private class RoomCreatedNotification
  {
    public string type;
    public string room_id;
    public string web_url;
  }

  private class GameEventNotification
  {
    public string type;
    public string event_type;
    public int trigger_count;
  }

  public  class GameEndSummaryNotification
  {
    public const string AllKey = "all";
    public const string EnemyKey = "enemy";
    public const string SkillKey = "skill";
    
    public Dictionary<string, SummaryDetail> SummaryDetails = new Dictionary<string, SummaryDetail>();
    
    public class SummaryDetail
    {
      public int count;
      public string viewer_id;
      public string viewer_name;
    }
  }
}

public enum FrontKey
{
  skill1,
  skill2,
  skill3,
  enemy1, 
  enemy2,
  enemy3,
}

///<summary>
/// WebSocketメッセージのタイプを表すEnum
///</summary>
public enum MessageType
{
  room_created,
  game_event,
  game_end_summary,
  unknown,
}

interface IWebSocketManager
{
  public ReadOnlyReactiveProperty<bool> IsConnectedProp { get; }
  public IReadOnlyDictionary<FrontKey, Subject<Unit>> FrontEventDict { get; }
  public UniTask ConnectWebSocketAsync(string websocketId = null);
  public UniTask DisconnectWebSocketAsync();
  public UniTask<string> GetFrontUrlAsync();
  public UniTask GameEndAsync();
  public void HealthCheck();
}