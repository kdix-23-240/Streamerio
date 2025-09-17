using UnityEngine;

public class PlayerView : MonoBehaviour
{
    public void Move(Vector2 newPosition)
    {
        Debug.Log($"PlayerView Move to {newPosition}");
        gameObject.transform.position += new Vector3(newPosition.x, newPosition.y, 0);
    }

    public void Jump(float force)
    {
        Debug.Log($"PlayerView Jump with force {force}");
        // ジャンプの実装例（Rigidbody2Dがアタッチされている場合）
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(new Vector2(0, force), ForceMode2D.Impulse);
        }
    }
}