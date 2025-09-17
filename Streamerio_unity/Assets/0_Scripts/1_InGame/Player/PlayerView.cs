using UnityEngine;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour
{
    public void Initialize(float initialPosX, float initialPosY)
    {
        UpdatePosition(initialPosX, initialPosY);
    }
    public void UpdatePosition(float x, float y)
    {
        transform.position = new Vector3(x, y, transform.position.z);
    }
}