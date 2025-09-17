using UnityEngine;
using R3;

public class PlayerPresenter
{
    private PlayerModel _model;
    [SerializeField] private PlayerView _view;

    void Awake()
    {
        // ModelとViewの初期化
        _model = new PlayerModel(_view.gameObject.transform.position.x, _view.gameObject.transform.position.y);
        _view.Initialize(_model._posX.Value, _model._posY.Value);
    }

    void Start()
    {
        Bind();
    }

    private void Bind()
    {
        _model._posX.Subscribe(x => _view.UpdatePosition(x, _model._posY.Value));
        _model._posY.Subscribe(y => _view.UpdatePosition(_model._posX.Value, y));
    }

    public void Move(Vector2 delta)
    {
        _model.Move(delta);
    }
}