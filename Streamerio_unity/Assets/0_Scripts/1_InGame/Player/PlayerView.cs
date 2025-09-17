using UnityEngine;

public class PlayerView : MonoBehaviour
{
    public void Move(Vector2 newPosition)
    {
        Debug.Log($"PlayerView Move to {newPosition}");
        gameObject.transform.position += new Vector3(newPosition.x, newPosition.y, 0);
    }
}