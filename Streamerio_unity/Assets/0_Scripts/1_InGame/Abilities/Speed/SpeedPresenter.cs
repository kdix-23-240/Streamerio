using UnityEngine;
using UniRx;

public class SpeedPresenter : MonoBehaviour, IAbility
{
    private SpeedModel _speedModel;
    public ReactiveProperty<float> Amount => _speedModel.CurrentSpeed;
    [SerializeField] private SpeedTestView _speedTestView;
    [SerializeField] private PlayerScriptableObject _scriptableObject;

    private float _currentSpeed;
    private float _maxSpeed;

    void Awake()
    {
        _currentSpeed = _scriptableObject.InitialSpeed;
        _maxSpeed = _scriptableObject.MaxSpeed;
        _speedModel = new SpeedModel(_currentSpeed, _maxSpeed);
        _speedTestView.Initialize(_currentSpeed);
    }

    void Start()
    {
        Bind();
    }

    void Bind()
    {
        _speedModel.CurrentSpeed.Subscribe(Speed =>
        {
            _speedTestView.UpdateView(Speed);
        }).AddTo(this);
    }

    public void Increase(float amount)
    {
        _speedModel.Increase(amount);
    }

    public void Decrease(float amount)
    {
        _speedModel.Decrease(amount);
    }
}