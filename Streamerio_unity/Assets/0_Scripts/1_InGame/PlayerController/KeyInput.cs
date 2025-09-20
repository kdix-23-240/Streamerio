using UnityEngine;

public class KeyInput : MonoBehaviour, IController
{
    [SerializeField] private PlayerPresenter _player;
    [SerializeField] private BulletShooter _bulletShooter;

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        // float moveY = Input.GetAxis("Vertical");
        Move(new Vector2(moveX, 0));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
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
    }
}