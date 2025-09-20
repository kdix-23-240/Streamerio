using System.Threading;
using Alchemy.Inspector;
using Cysharp.Text;
using R3;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField, ReadOnly]
    private Animator _animator;

    [SerializeField] private float _moveDelay = 0.5f;

    private bool _isMoving = false;
    private bool _isCancelMoving = false;
    private float _moveZeroTime = 0f;
    private CancellationTokenSource _cts;

#if UNITY_EDITOR
    private void OnValidate()
    {
        _animator ??= GetComponent<Animator>();
    }
#endif

    public void PlayRun(float value)
    {
        bool isMoving = value != 0;
        if (isMoving == _isMoving)
        {
            return;
        }
        else if (isMoving)
        {
            _animator.SetBool("IsMove", true);
        }
        else if (_isCancelMoving)
        {
            _cts?.Cancel();
        }

        CancelMove();
    }

    private void CancelMove()
    {
        _cts = new CancellationTokenSource();
        _isCancelMoving = false;
        
        float time = 0f;
        Observable.EveryUpdate()
            .Select(_ => Time.deltaTime)
            .Subscribe(value =>
            {
                time += value;
                if (time >= _moveDelay)
                {
                    _animator.SetBool("IsMove", false);
                    _cts?.Cancel();
                    _isCancelMoving = false;
                }
            }).RegisterTo(_cts.Token);
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
