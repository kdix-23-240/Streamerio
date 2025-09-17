using UnityEngine;
using R3;

public class HpPresenter : MonoBehaviour, IAbility
{
    private HpModel _hpModel;
    public float Amount => _hpModel.CurrentHp.Value;
    [SerializeField] private HpTestView _hpTestView;
    [SerializeField] private PlayerScriptableObject _scriptableObject;

    private float _currentHp;
    private float _maxHp;

    void Awake()
    {
        _currentHp = _scriptableObject.InitialHp;
        _maxHp = _scriptableObject.MaxHp;
        _hpModel = new HpModel(_currentHp, _maxHp);
        _hpTestView.Initialize(_currentHp);
    }

    void Start()
    {
        Bind();
    }

    void Bind()
    {
        _hpModel.CurrentHp.Subscribe(hp =>
        {
            _hpTestView.UpdateView(hp);
        }).AddTo(this);
    }

    public void Increase(float amount)
    {
        _hpModel.Increase(amount);
    }

    public void Decrease(float amount)
    {
        _hpModel.Decrease(amount);
    }
}