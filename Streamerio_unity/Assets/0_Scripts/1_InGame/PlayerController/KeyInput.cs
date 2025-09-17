using UnityEngine;

public class KeyInput : MonoBehaviour, IController
{
    [SerializeField] private PlayerPresenter _player;
    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        // float moveY = Input.GetAxis("Vertical");
        Move(new Vector2(moveX, 0));
    }

    public void Move(Vector2 direction)
    {
        _player.Move(direction);
    }
}