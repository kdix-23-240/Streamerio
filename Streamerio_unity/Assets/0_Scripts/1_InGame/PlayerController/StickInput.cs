using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Common.UI.Part.Button;

public class StickInput : MonoBehaviour, IController
{
    [SerializeField] private PlayerPresenter _player;
    [SerializeField] private BulletShooter _bulletShooter;
    [SerializeField] private Joystick _joystick;
    [SerializeField] private CommonButton _jumpButton;
    [SerializeField] private CommonButton _attackButton;

    void Start()
    {
        _jumpButton.Initialize();
        _attackButton.Initialize();
        _jumpButton.SetClickEvent(Jump);
        _attackButton.SetClickEvent(Attack);
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
        AudioManager.Instance.PlayAsync(SEType.PlayerAttack, destroyCancellationToken).Forget();
        _bulletShooter.Shoot();
    }
}