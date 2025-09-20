using Alchemy.Inspector;
using Cysharp.Text;
using R3;
using UnityEngine;

[RequireComponent(typeof(PlayerAnimation))]
public class PlayerView : MonoBehaviour
{
    [SerializeField, ReadOnly]
    private SpriteRenderer _spriteRenderer;
    [SerializeField, ReadOnly]
    private Rigidbody2D _rb2D;
    [SerializeField, LabelText("地面判定用コライダー")]
    private PlayerGrounded _groundedCollider;
    [SerializeField, LabelText("アニメーション"), ReadOnly]
    private PlayerAnimation _animation;
    
    // private bool _isGrounded = false; // 地面に接地しているかどうかのフラグ
    // public bool IsGrounded => _isGrounded;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        _spriteRenderer ??= GetComponent<SpriteRenderer>();
        _animation ??= GetComponent<PlayerAnimation>();
        _rb2D ??= GetComponent<Rigidbody2D>();
        _groundedCollider ??= GetComponentInChildren<PlayerGrounded>();
    }
#endif

    public void Bind()
    {
        _groundedCollider.IsGrounded
            .Where(x => !x)
            .DistinctUntilChanged()
            .Subscribe(_ =>
            {
                _animation.PlayJump(false);
            }).RegisterTo(destroyCancellationToken);
    }
    
    public void Move(Vector2 newPosition)
    {
        // Debug.Log($"PlayerView Move to {newPosition}");
        _animation.PlayRun(newPosition.x != 0);
        
        _spriteRenderer.flipX = newPosition.x < 0;
        
        gameObject.transform.position += new Vector3(newPosition.x, newPosition.y, 0);
    }

    public void Jump(float force)
    {
        if (!_groundedCollider.IsGrounded.CurrentValue) return;
        // Debug.Log($"PlayerView Jump with force {force}");
        // ジャンプの実装例（Rigidbody2Dがアタッチされている場合）
        
        _animation.PlayJump(true);
        _rb2D.AddForce(new Vector2(0, force), ForceMode2D.Impulse);
    }

    /// <summary>
    /// 地面に接地しているかどうかをチェックするロジックをここに実装
    /// y軸方向の速度が0に近い場合など
    /// </summary>
    // private bool CheckIsGrounded()
    // {
    //     return Mathf.Abs(_rb2D.linearVelocity.y) < 0.01f;
    // }
    
    public void Attack(int num)
    {
        _animation.PlayAttack(num);
    }
}