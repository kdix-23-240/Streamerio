using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace InGame.Skill.Audio
{
    public class AudioPlayer: MonoBehaviour
    {
        [SerializeField]
        private SEType _seType;
        
        public void PlaySE()
        {
            AudioManager.Instance.PlayAsync(_seType, destroyCancellationToken).Forget();
        }
    }
}