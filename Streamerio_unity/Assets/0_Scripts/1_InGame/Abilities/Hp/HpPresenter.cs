using Common.Audio;
using Cysharp.Threading.Tasks;
using InGame;
using InGame.UI.Heart;
using UnityEngine;
using R3;

public class HpPresenter : MonoBehaviour, IAbility
{
    private HpModel _hpModel;
    public float Amount => _hpModel.CurrentHp.Value;
    [SerializeField] private HeartGroupView _hpView;
    [SerializeField] private PlayerScriptableObject _scriptableObject;

    private float _currentHp;
    private float _maxHp;

    void Awake()
    {
        _currentHp = _scriptableObject.InitialHp;
        _maxHp = _scriptableObject.MaxHp;
        _hpModel = new HpModel(_currentHp, _maxHp);
        _hpView.Initialize(0, _currentHp);
    }

    void Start()
    {
        Bind();
    }

    void Bind()
    {
        _hpModel.CurrentHp.Subscribe(hp =>
        {
            _hpView.UpdateHP(hp);
            if (hp <= 0)
            {
                InGameManager.Instance.GameOver();
            }
        }).AddTo(this);
    }

    public void Increase(float amount)
    {
        _hpModel.Increase(amount);
    }

    public void Decrease(float amount)
    {
        AudioManager.Instance.PlayAsync(SEType.PlayerDamage, destroyCancellationToken).Forget();
        _hpModel.Decrease(amount);
    }
}