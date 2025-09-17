using UnityEngine;

public class PlayerView : MonoBehaviour
{
    private bool _isGrounded = false; // 地面に接地しているかどうかのフラグ
    public void Move(Vector2 newPosition)
    {
        Debug.Log($"PlayerView Move to {newPosition}");
        gameObject.transform.position += new Vector3(newPosition.x, newPosition.y, 0);
    }

    public void Jump(float force)
    {
        if (!CheckIsGrounded()) return;
        Debug.Log($"PlayerView Jump with force {force}");
        // ジャンプの実装例（Rigidbody2Dがアタッチされている場合）
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(new Vector2(0, force), ForceMode2D.Impulse);
        }
    }

    /// <summary>
    /// 地面に接地しているかどうかをチェックするロジックをここに実装
    /// y軸方向の速度が0に近い場合など
    /// </summary>
    private bool CheckIsGrounded()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            return Mathf.Abs(rb.linearVelocity.y) < 0.01f;
        }
        return false;
    }
}