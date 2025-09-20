using Alchemy.Inspector;
using Cysharp.Text;
using UnityEngine;

[RequireComponent(typeof(PlayerAnimation))]
public class PlayerView : MonoBehaviour
{
    [SerializeField, LabelText("アニメーション"), ReadOnly]
    private PlayerAnimation _animation;
    
    private bool _isGrounded = false; // 地面に接地しているかどうかのフラグ
    public bool IsGrounded => _isGrounded;

#if UNITY_EDITOR
    private void OnValidate()
    {
        _animation ??= GetComponent<PlayerAnimation>();
    }
#endif
    
    public void Move(Vector2 newPosition)
    {
        // Debug.Log($"PlayerView Move to {newPosition}");
        _animation.PlayRun(newPosition.x != 0);
        
        gameObject.transform.position += new Vector3(newPosition.x, newPosition.y, 0);

        if (CheckIsGrounded())
        {
            _animation.PlayJump(false);
        }
    }

    public void Jump(float force)
    {
        if (!CheckIsGrounded()) return;
        // Debug.Log($"PlayerView Jump with force {force}");
        // ジャンプの実装例（Rigidbody2Dがアタッチされている場合）
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            _animation.PlayJump(true);
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

    public void Attack(int num)
    {
        _animation.PlayAttack(num);
    }
}