using Alchemy.Inspector;
using Cysharp.Text;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField, ReadOnly]
    private Animator _animator;

#if UNITY_EDITOR
    private void OnValidate()
    {
        _animator ??= GetComponent<Animator>();
    }
#endif

    public void PlayRun(bool isMove)
    {
        _animator.SetBool("IsMove", isMove);
    }

    public void PlayJump(bool isJump)
    {
        _animator.SetBool("IsJump", isJump);
    }

    public void PlayAttack(int num)
    {
        var key = ZString.Concat("Attack", num);
        _animator.SetTrigger(key);
    }

    public void PlayDeath()
    {
        _animator.SetTrigger("Death");
    }
}
