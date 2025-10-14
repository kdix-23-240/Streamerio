using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// Skelton敵キャラクター用のVContainerインストーラー
/// </summary>
public class SkeltonInstaller : LifetimeScope
{
    [Header("Skelton Configuration")]
    [SerializeField] private SkeltonScriptableObject skeltonConfig;

    protected override void Configure(IContainerBuilder builder)
    {
        if (skeltonConfig == null)
        {
            Debug.LogError($"[SkeltonInstaller] SkeltonScriptableObject is not assigned on {gameObject.name}");
            return;
        }

        // SkeltonScriptableObjectをIEnemyStatsとして登録
        builder.RegisterInstance<IEnemyStats>(skeltonConfig);
        
        // Skelton自体をコンポーネントとして登録
        builder.RegisterComponentInHierarchy<Skelton>();
        builder.RegisterComponentInHierarchy<EnemyHpManager>();
    }

    // フォールバック用のメソッド
    public IEnemyStats GetSkeltonConfig()
    {
        return skeltonConfig;
    }
}