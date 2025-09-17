using UnityEngine;
using R3;

public class PlayerModel
{
    public ReactiveProperty<float> _posX;
    public ReactiveProperty<float> _posY;

    public PlayerModel(float initialPosX, float initialPosY)
    {
        _posX = new ReactiveProperty<float>(initialPosX);
        _posY = new ReactiveProperty<float>(initialPosY);
    }
    
    public void Move(Vector2 delta)
    {
        _posX.Value += delta.x;
        _posY.Value += delta.y;
    }
}