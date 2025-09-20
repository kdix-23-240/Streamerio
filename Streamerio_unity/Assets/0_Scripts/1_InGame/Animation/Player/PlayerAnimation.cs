using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator _animator;
    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Start()
    {

    }

    public void PlayIdle()
    {
        _animator.SetBool("isMove", false);
        _animator.SetBool("isJump", false);
    }

    public void PlayRun()
    {
        _animator.SetBool("isMove", true);
    }

    public void PlayJump()
    {
        _animator.SetBool("isJump", true);
    }

    public void PlayAttack1()
    {
        _animator.SetTrigger("Attack1");
    }

    public void PlayAttack2()
    {
        _animator.SetTrigger("Attack2");
    }

}
