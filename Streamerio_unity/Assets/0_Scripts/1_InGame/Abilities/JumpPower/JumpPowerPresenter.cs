using UnityEngine;
using R3;

public class JumpPowerPresenter : MonoBehaviour, IAbility
{
    private JumpPowerModel _jumpPowerModel;
    public float Amount => _jumpPowerModel.CurrentJumpPower.Value;
    [SerializeField] private JumpPowerTestView _jumpPowerTestView;
    [SerializeField] private PlayerScriptableObject _scriptableObject;

    private float _currentJumpPower;
    private float _maxJumpPower;

    void Awake()
    {
        _currentJumpPower = _scriptableObject.InitialJumpPower;
        _maxJumpPower = _scriptableObject.MaxJumpPower;
        _jumpPowerModel = new JumpPowerModel(_currentJumpPower, _maxJumpPower);
        _jumpPowerTestView.Initialize(_currentJumpPower);
    }

    void Start()
    {
        Bind();
    }

    void Bind()
    {
        _jumpPowerModel.CurrentJumpPower.Subscribe(JumpPower =>
        {
            _jumpPowerTestView.UpdateView(JumpPower);
        }).AddTo(this);
    }

    public void Increase(float amount)
    {
        _jumpPowerModel.Increase(amount);
    }

    public void Decrease(float amount)
    {
        _jumpPowerModel.Decrease(amount);
    }
}