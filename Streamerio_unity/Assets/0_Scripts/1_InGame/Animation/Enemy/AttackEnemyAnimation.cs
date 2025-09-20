using UnityEngine;
using Cysharp.Threading.Tasks;

public class AttackEnemyAnimation : MonoBehaviour
{
    [SerializeField] int _attackDuration = 2000;
    private Animator _animator;
    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            PlayAttack();
        }
    }

    private async UniTask PlayAttack()
    {
        _animator.SetBool("isAttack", true);
        await UniTask.Delay(_attackDuration);
        _animator.SetBool("isAttack", false);
    }
}