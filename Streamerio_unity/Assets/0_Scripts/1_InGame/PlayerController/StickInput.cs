using UnityEngine;
using UnityEngine.UI;

public class StickInput : MonoBehaviour, IController
{
    [SerializeField] private PlayerPresenter _player;
    [SerializeField] private BulletShooter _bulletShooter;
    [SerializeField] private Joystick _joystick;
    [SerializeField] private Button _jumpButton;
    [SerializeField] private Button _attackButton;

    void Start()
    {
        _jumpButton.onClick.AddListener(Jump);
        _attackButton.onClick.AddListener(Attack);
    }

    void Update()
    {
        float moveX = _joystick.Horizontal;
        Move(new Vector2(moveX, 0));
    }

    public void Move(Vector2 direction)
    {
        _player.Move(direction);
    }

    public void Jump()
    {
        _player.Jump();
    }

    public void Attack()
    {
        _bulletShooter.Shoot();
    }
}