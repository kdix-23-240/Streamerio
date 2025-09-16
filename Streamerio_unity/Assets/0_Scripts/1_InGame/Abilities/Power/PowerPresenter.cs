using UnityEngine;
using UniRx;

public class PowerPresenter : MonoBehaviour, IAbility
{
    private PowerModel _powerModel;
    public ReactiveProperty<float> Amount => _powerModel.CurrentPower;
    [SerializeField] private PowerTestView _powerTestView;
    [SerializeField] private PlayerScriptableObject _scriptableObject;

    private float _currentPower;
    private float _maxPower;

    void Awake()
    {
        _currentPower = _scriptableObject.InitialPower;
        _maxPower = _scriptableObject.MaxPower;
        _powerModel = new PowerModel(_currentPower, _maxPower);
        _powerTestView.Initialize(_currentPower);
    }

    void Start()
    {
        Bind();
    }

    void Bind()
    {
        _powerModel.CurrentPower.Subscribe(Power =>
        {
            _powerTestView.UpdateView(Power);
        }).AddTo(this);
    }

    public void Increase(float amount)
    {
        _powerModel.Increase(amount);
    }

    public void Decrease(float amount)
    {
        _powerModel.Decrease(amount);
    }
}