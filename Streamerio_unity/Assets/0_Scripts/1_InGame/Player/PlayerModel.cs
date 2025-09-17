public class PlayerModel
{
    private float _posX;
    public float PosX { get => _posX; set => _posX = value; }
    private float _posY;
    public float PosY { get => _posY; set => _posY = value; }

    public PlayerModel(float initialPosX, float initialPosY)
    {
        PosX = initialPosX;
        PosY = initialPosY;
    }
}