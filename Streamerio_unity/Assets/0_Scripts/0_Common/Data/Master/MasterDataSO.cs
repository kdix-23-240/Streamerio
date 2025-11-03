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
    
    [CreateAssetMenu(fileName = "MasterDataSO", menuName = "SO/Master")]
    public class MasterDataSO : ScriptableObject, IMasterData
    {
#if UNITY_EDITOR
        [Button]
        private async UniTaskVoid FetchData()
        {
            await FetchDataAsync(default);
            
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
#endif
        
        [SerializeField, Min(0f)]
        private int _timeOutTime = 8;
        
        [SerializeField]
        private MasterGameSetting[] _gameSetting;
        public MasterGameSetting[] GameSetting => _gameSetting;

        [SerializeField] private MasterPlayerStatus[] _playerStatus; 
        public MasterPlayerStatus[] PlayerStatus => _playerStatus;
        [SerializeField]
        private MasterUltStatus[] _ultStatus;
        public MasterUltStatus[] UltStatus => _ultStatus;
        [SerializeField]
        private SerializeDictionary<MasterEnemyType, MasterEnemyStatus> _enemyStatusDict;
        public IReadOnlyDictionary<MasterEnemyType, MasterEnemyStatus> EnemyStatusDict => _enemyStatusDict.ToDictionary();

        public async UniTask FetchDataAsync(CancellationToken ct)
        {
            var gameTask    = SpreadSheetClient.GetRequestAsync(SheetType.GameSettings, ct, _timeOutTime);
            var playerTask  = SpreadSheetClient.GetRequestAsync(SheetType.PlayerStatus, ct, _timeOutTime);
            var ultTask     = SpreadSheetClient.GetRequestAsync(SheetType.UltStatus, ct, _timeOutTime);
            var enemyTask   = SpreadSheetClient.GetRequestAsync(SheetType.EnemyStatus, ct, _timeOutTime);

            var (gameRows, playerRows, ultRows, enemyRows) =
                await UniTask.WhenAll(gameTask, playerTask, ultTask, enemyTask);

            int gameCount = gameRows[MasterGameSetting.TimeLimitKey].Count;
            _gameSetting = new MasterGameSetting[gameCount];
            for(int i = 0; i < gameCount; i++)
            {
                _gameSetting[i] = new MasterGameSetting()
                {
                    TimeLimit = ToFloat(gameRows[MasterGameSetting.TimeLimitKey][i]),
                };
            }
            
            int playerCount = playerRows[MasterPlayerStatus.HPKey].Count;
            _playerStatus = new MasterPlayerStatus[playerCount];
            for (int i = 0; i < playerCount; i++)
            {
                PlayerStatus[i] = new MasterPlayerStatus()
                {
                    HP = ToFloat(playerRows[MasterPlayerStatus.HPKey][i]),
                    AttackPower = ToFloat(playerRows[MasterPlayerStatus.AttackPowerKey][i]),
                    Speed = ToFloat(playerRows[MasterPlayerStatus.SpeedKey][i]),
                    JumpPower = ToFloat(playerRows[MasterPlayerStatus.JumpPowerKey][i]),
                };
            }
            
            int ultCount = ultRows[MasterUltStatus.AttackPowerKey].Count;
            _ultStatus = new MasterUltStatus[ultCount];
            for (int i = 0; i < ultCount; i++)
            {
                UltStatus[i] = new MasterUltStatus()
                {
                    AttackPower = ToFloat(ultRows[MasterUltStatus.AttackPowerKey][i]),
                };
            }
            
            _enemyStatusDict = new SerializeDictionary<MasterEnemyType, MasterEnemyStatus>();
            int enemyCount = enemyRows[MasterEnemyStatus.HPKey].Count;
            for (int i = 0; i < enemyCount; i++)
            {
                var type = (MasterEnemyType)Enum.Parse(typeof(MasterEnemyType), (string)enemyRows[MasterEnemyStatus.EnemyTypeKey][i]);
                _enemyStatusDict[type] = new MasterEnemyStatus()
                {
                    HP = ToFloat(enemyRows[MasterEnemyStatus.HPKey][i]),
                    AttackPower = ToFloat(enemyRows[MasterEnemyStatus.AttackPowerKey][i]),
                    Speed = ToFloat(enemyRows[MasterEnemyStatus.SpeedKey][i]),
                };
            }
        }
        
        private float ToFloat(object num)
        {
            return Convert.ToSingle(num, CultureInfo.InvariantCulture);
        }
    }
    
    public interface IMasterData
    {
        MasterGameSetting[] GameSetting { get; }
        MasterPlayerStatus[] PlayerStatus { get; }
        MasterUltStatus[] UltStatus { get; }
        IReadOnlyDictionary<MasterEnemyType, MasterEnemyStatus> EnemyStatusDict { get; }
        
        UniTask FetchDataAsync(CancellationToken ct);
    }
    
    [Serializable]
    public class MasterGameSetting
    {
        public const string TimeLimitKey = "TimeLimit";
        
        public float TimeLimit;
    }
    
    [Serializable]
    public class MasterPlayerStatus
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
    public class MasterUltStatus
    {
        public const string AttackPowerKey = "AttackPower";
        
        public float AttackPower;
    }

    [Serializable]
    public class MasterEnemyStatus
    {
        public const string EnemyTypeKey = "Type";
        public const string HPKey = "HP";
        public const string AttackPowerKey = "AttackPower";
        public const string SpeedKey = "Speed";
        
        public float HP;
        public float AttackPower;
        public float Speed;
    }
    
    public enum MasterEnemyType
    {
        Skelton,
        FireMan,
        Cat,
    }
}