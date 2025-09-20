using UnityEngine;

public class Heal : MonoBehaviour
{
    [Header("ステータス")]
    [SerializeField] private int healPower = 10;

    [Header("ダメージ適用先 (プレイヤーHP)")]
    [SerializeField] private HpPresenter hpPresenter; // プレイヤーの HpPresenter を直接アタッチ

    void Awake()
    {
        if (hpPresenter == null)
        {
            Debug.LogWarning("[WeakEnemy] hpPresenter 未割当です。シーン上のプレイヤーHPオブジェクトをインスペクタでセットしてください。");
        }
    }

    void Update()
    {
        
    }

    private void HealPlayer()
    {
        if (hpPresenter == null) return;
        hpPresenter.Increase(healPower);
        Debug.Log($"Player healed! Player HP: {hpPresenter.Amount} (+{healPower})");
    }
}