using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Alchemy.Inspector;
using Common.GAS;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common
{
    [CreateAssetMenu(fileName = "OnlineStatusDataSO", menuName = "SO/Online/Status")]
    public class OnlineStatusDataSO : ScriptableObject
    {
        [Button]
        private async UniTaskVoid FetchData()
        {
            await GetData(default);
        }
        
        [SerializeField, Min(0f)]
        private int _timeOutTime = 8;
        
        public OnlineGameSetting[] GameSetting;
        public OnlinePlayerStatus[] PlayerStatus;
        public OnlineUltStatus[] UltStatus;
        [SerializeField]
        private SerializeDictionary<OnlineEnemyType, OnlineEnemyStatus> _enemyStatusDict;
        public Dictionary<OnlineEnemyType, OnlineEnemyStatus> EnemyStatusDict => _enemyStatusDict.ToDictionary();

        public async UniTask GetData(CancellationToken ct)
        {
            var gameTask    = SpreadSheetClient.GetRequestAsync(SheetType.GameSettings, ct, _timeOutTime);
            var playerTask  = SpreadSheetClient.GetRequestAsync(SheetType.PlayerStatus, ct, _timeOutTime);
            var ultTask     = SpreadSheetClient.GetRequestAsync(SheetType.UltStatus, ct, _timeOutTime);
            var enemyTask   = SpreadSheetClient.GetRequestAsync(SheetType.EnemyStatus, ct, _timeOutTime);

            var (gameRows, playerRows, ultRows, enemyRows) =
                await UniTask.WhenAll(gameTask, playerTask, ultTask, enemyTask);

            int gameCount = gameRows[OnlineGameSetting.TimeLimitKey].Count;
            GameSetting = new OnlineGameSetting[gameCount];
            for(int i = 0; i < gameCount; i++)
            {
                GameSetting[i] = new OnlineGameSetting()
                {
                    TimeLimit = ToFloat(gameRows[OnlineGameSetting.TimeLimitKey][i]),
                };
            }
            
            int playerCount = playerRows[OnlinePlayerStatus.HPKey].Count;
            PlayerStatus = new OnlinePlayerStatus[playerCount];
            for (int i = 0; i < playerCount; i++)
            {
                PlayerStatus[i] = new OnlinePlayerStatus()
                {
                    HP = ToFloat(playerRows[OnlinePlayerStatus.HPKey][i]),
                    AttackPower = ToFloat(playerRows[OnlinePlayerStatus.AttackPowerKey][i]),
                    Speed = ToFloat(playerRows[OnlinePlayerStatus.SpeedKey][i]),
                    JumpPower = ToFloat(playerRows[OnlinePlayerStatus.JumpPowerKey][i]),
                };
            }
            
            int ultCount = ultRows[OnlineUltStatus.AttackPowerKey].Count;
            UltStatus = new OnlineUltStatus[ultCount];
            for (int i = 0; i < ultCount; i++)
            {
                UltStatus[i] = new OnlineUltStatus()
                {
                    AttackPower = ToFloat(ultRows[OnlineUltStatus.AttackPowerKey][i]),
                };
            }
            
            _enemyStatusDict = new SerializeDictionary<OnlineEnemyType, OnlineEnemyStatus>();
            int enemyCount = enemyRows[OnlineEnemyStatus.HPKey].Count;
            for (int i = 0; i < enemyCount; i++)
            {
                var type = (OnlineEnemyType)Enum.Parse(typeof(OnlineEnemyType), (string)enemyRows[OnlineEnemyStatus.EnemyTypeKey][i]);
                _enemyStatusDict[type] = new OnlineEnemyStatus()
                {
                    HP = ToFloat(enemyRows[OnlineEnemyStatus.HPKey][i]),
                    AttackPower = ToFloat(enemyRows[OnlineEnemyStatus.AttackPowerKey][i]),
                    Speed = ToFloat(enemyRows[OnlineEnemyStatus.SpeedKey][i]),
                };
            }
        }
        
        private float ToFloat(object num)
        {
            return Convert.ToSingle(num, CultureInfo.InvariantCulture);
        }
    }
    
    [Serializable]
    public class OnlineGameSetting
    {
        public const string TimeLimitKey = "TimeLimit";
        
        public float TimeLimit;
    }
    
    [Serializable]
    public class OnlinePlayerStatus
    {
        public const string HPKey = "HP";
        public const string AttackPowerKey = "AttackPower";
        public const string SpeedKey = "Speed";
        public const string JumpPowerKey = "JumpPower";
        
        public float HP;
        public float AttackPower;
        public float Speed;
        public float JumpPower;
    }

    [Serializable]
    public class OnlineUltStatus
    {
        public const string AttackPowerKey = "AttackPower";
        
        public float AttackPower;
    }

    [Serializable]
    public class OnlineEnemyStatus
    {
        public const string EnemyTypeKey = "Type";
        public const string HPKey = "HP";
        public const string AttackPowerKey = "AttackPower";
        public const string SpeedKey = "Speed";
        
        public float HP;
        public float AttackPower;
        public float Speed;
    }
    
    public enum OnlineEnemyType
    {
        Skelton,
        FireMan,
        Cat,
    }
}