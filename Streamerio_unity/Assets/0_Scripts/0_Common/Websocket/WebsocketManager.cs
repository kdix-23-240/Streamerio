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

public class WebSocketManager : SingletonBase<WebSocketManager>, IWebSocketManager
{
  private WebSocket _websocket;

  private ReactiveProperty<bool> _isConnectedProp = new ReactiveProperty<bool>(false);
  public ReadOnlyReactiveProperty<bool> IsConnectedProp => _isConnectedProp;

  private string _roomId = string.Empty;

  private Dictionary<FrontKey, Subject<Unit>> _frontEventDict = new Dictionary<FrontKey, Subject<Unit>>();
  public IReadOnlyDictionary<FrontKey, Subject<Unit>> FrontEventDict => _frontEventDict;
  
  private GameEndSummaryNotification _gameEndSummary = null;
  public GameEndSummaryNotification GameEndSummary => _gameEndSummary;

  // アプリケーション終了中フラグ
  private bool _isShuttingDown = false;
  
  [SerializeField]
  private ApiConfigSO _apiConfigSO;
  
  private string _qrCodeURL = string.Empty;

  private string _frontendUrlFormat = null; 

  private string _backendHttpUrl = null;

  private string _backendWsBaseUrl = null;

  protected override void Awake()
  {
    base.Awake();
    
    if (_apiConfigSO != null)
    {
      _frontendUrlFormat = _apiConfigSO.frontendUrlFormat;
      _backendHttpUrl = _apiConfigSO.backendHttpUrl;
      _backendWsBaseUrl = _apiConfigSO.backendWsUrl;
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

  private void Start()
  {
    Observable.EveryUpdate()
      .Select(_ => _websocket != null && _websocket.State == WebSocketState.Open)
      .DistinctUntilChanged()
      .Subscribe(isConnected =>
      {
        _isConnectedProp.Value = isConnected;
      })
      .AddTo(this);
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
  public async UniTask ConnectWebSocket([CanBeNull] string websocketId)
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
      websocketUrl = ZString.Format("{0}?room_id={1}", _backendWsBaseUrl, websocketId);
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
    };

    _websocket.OnError += (e) =>
    {
      Debug.Log("Error! " + e);
    };

    _websocket.OnClose += async (e) =>
    {
      Debug.Log("Connection closed!");
      
      // アプリケーション終了中は再接続しない
      if (_isShuttingDown) return;
      
      // 再接続を試行
      // 現在のwebsocketIdが空の場合は新しくwebsocketIdを生成して接続
      await ConnectWebSocket(_roomId ?? string.Empty);
    };

    _websocket.OnMessage += (bytes) => ReceiveWebSocketMessage(bytes);

    await _websocket.Connect();
    return;
  }

  ///<summary>
  /// WebSocketからメッセージを受信する
  ///</summary>
  private void ReceiveWebSocketMessage(byte[] bytes)
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
          _roomId = room.room_id;
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
          var keyType = (FrontKey)System.Enum.Parse(typeof(FrontKey), gameEvent.event_type, true);
          _frontEventDict[keyType]?.OnNext(Unit.Default);
        }
        
        return;
      }
      catch (Exception ex)
      {
        Debug.Log($"game_event parse error: {ex.Message}");
      }
      Debug.Log("Failed to parse game_event message.");
      return;
    }
    if (baseMessage.type == "game_end_summary")
    {
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
        
        return;
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

  ///<summary>
  /// WebSocketを切断する
  ///</summary>
  public async UniTask DisconnectWebSocket()
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
  public async UniTask GameEnd()
  {
    await SendWebSocketMessage( "{\"type\": \"game_end\" }" );
  }


  ///<summary>
  /// UnityからWebSocketにメッセージを送信する
  ///</summary>
  private async UniTask SendWebSocketMessage(string message)
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
  /// アプリケーションが終了したときにwebsocketを閉じる
  ///</summary>
  private async void OnApplicationQuit()
  {
    _isShuttingDown = true;
    try
    {
      await DisconnectWebSocket();
    }
    catch (Exception ex)
    {
      Debug.Log($"Error during WebSocket disconnection: {ex.Message}");
    }
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

interface IWebSocketManager
{
  public ReadOnlyReactiveProperty<bool> IsConnectedProp { get; }
  public IReadOnlyDictionary<FrontKey, Subject<Unit>> FrontEventDict { get; }
  public UniTask ConnectWebSocket(string websocketId);
  public UniTask DisconnectWebSocket();
  public UniTask<string> GetFrontUrlAsync();
  public UniTask GameEnd();
  public void HealthCheck();
}