using UnityEngine;
using R3;

public class JumpPresenter : MonoBehaviour, IAbility
{
    private JumpModel _jumpModel;
    public ReactiveProperty<float> Amount => _jumpModel.CurrentJumpPower;
    [SerializeField] private JumpTestView _jumpTestView;
    [SerializeField] private PlayerScriptableObject _playerData;

    private float _currentJumpPower;
    private float _maxJumpPower;

    void Awake()
    {
        _currentJumpPower = _playerData.MaxJumpPower;
        _maxJumpPower = _playerData.MaxJumpPower;
        _jumpModel = new JumpModel(_currentJumpPower, _maxJumpPower);
        _jumpTestView.Initialize(_currentJumpPower);
    }

    void Start()
    {
        Bind();
    }

    void Bind()
    {
        _jumpModel.CurrentJumpPower.Subscribe(jumpPower =>
        {
            _jumpTestView.UpdateView(jumpPower);
        }).AddTo(this);
    }

    public void Increase(float amount)
    {
        _jumpModel.Increase(amount);
    }

    public void Decrease(float amount)
    {
        _jumpModel.Decrease(amount);
    }
}