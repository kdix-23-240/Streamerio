using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class SkeltonAnimation : MonoBehaviour
{
    [SerializeField] int _bornDuration = 2000;
    private Animator _animator;
    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        PlayBorn().Forget();
    }
    
    public async UniTask PlayBorn()
    {
        await UniTask.Delay(_bornDuration);
        _animator.SetBool("hasBorn", true);
    }
}