using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Common.UI.Part.Button;
using R3;
using VContainer;

public class StickInput : MonoBehaviour, IController
{
    [SerializeField] private PlayerPresenter _player;
    [SerializeField] private BulletShooter _bulletShooter;
    [SerializeField] private Joystick _joystick;
    private ICommonButton _jumpButton;
    private ICommonButton _attackButton;

    private IAudioFacade _audioFacade;
    
    [Inject]
    public void Construct([Key(ButtonType.Jump)] ICommonButton jumpButton,
                          [Key(ButtonType.Attack)] ICommonButton attackButton,
                          IAudioFacade audioFacade)
    {
        _jumpButton = jumpButton;
        _attackButton = attackButton;
        
        _audioFacade = audioFacade;
        
        _jumpButton.OnClickAsObservable
            .Subscribe(_ =>
            {
                Jump();
            }).RegisterTo(destroyCancellationToken);
        
        _attackButton.OnClickAsObservable
            .Subscribe(_ =>
            {
                Attack();
            }).RegisterTo(destroyCancellationToken);
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
        _audioFacade.PlayAsync(SEType.PlayerAttack, destroyCancellationToken).Forget();
        _bulletShooter.Shoot();
    }
}