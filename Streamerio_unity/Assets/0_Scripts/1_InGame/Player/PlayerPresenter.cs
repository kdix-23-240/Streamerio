using UnityEngine;
using R3;

public class PlayerPresenter : MonoBehaviour
{
    private PlayerModel _model;
    [SerializeField] private PlayerView _view;
    [SerializeField] private PlayerScriptableObject _playerData;

    void Awake()
    {
        // ModelとViewの初期化
        _model = new PlayerModel(_view.gameObject.transform.position.x, _view.gameObject.transform.position.y);
    }

    void Start()
    {
        Bind();
    }

    private void Bind()
    {
        Observable.EveryUpdate()
            .Select(_ => _view.gameObject.transform.position)
            .DistinctUntilChanged()
            .Subscribe(pos =>
            {
                _model.PosX = pos.x;
                _model.PosY = pos.y;
            })
            .AddTo(this);
    }

    public void Move(Vector2 delta)
    {
        _view.Move(delta * _playerData.InitialSpeed * Time.deltaTime);
    }

    public void Jump()
    {
        _view.Jump(_playerData.InitialJumpPower);
    }
}