using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AudioTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        AudioManager.Instance.Initialize();

        AudioManager.Instance.PlayAsync(BGMType.maou_bgm_neorock83, destroyCancellationToken).Forget();

        for (int i = 0; i < 11; i++)
        {
            AudioManager.Instance.PlayAsync(SEType.maou_se_system48, destroyCancellationToken).Forget();
        }

        await UniTask.WaitForSeconds(1);
        AudioManager.Instance.PlayAsync(SEType.maou_se_system48, destroyCancellationToken).Forget();
        await UniTask.WaitForSeconds(1);
        
        AudioManager.Instance.ChangeVolume(VolumeType.BGM, new Volume(25));
        
        AudioManager.Instance.ChangeVolume(VolumeType.SE, new Volume(25));
        AudioManager.Instance.PlayAsync(SEType.maou_se_system48, destroyCancellationToken).Forget();

        await UniTask.WaitForSeconds(0.5f);
        AudioManager.Instance.ChangeVolume(VolumeType.Master, new Volume(25));
        
        AudioManager.Instance.ChangeVolume(VolumeType.Master, new Volume(50));
        AudioManager.Instance.ChangeVolume(VolumeType.BGM, new Volume(50));
        AudioManager.Instance.ChangeVolume(VolumeType.SE, new Volume(50));
        
        AudioManager.Instance.ToggleMute(VolumeType.BGM);
        await UniTask.WaitForSeconds(1f);
        AudioManager.Instance.ToggleMute(VolumeType.BGM);
        
        AudioManager.Instance.PlayAsync(SEType.maou_se_system48, destroyCancellationToken).Forget();
        AudioManager.Instance.ToggleMute(VolumeType.SE);
        await UniTask.WaitForSeconds(1f);
        AudioManager.Instance.ToggleMute(VolumeType.SE);
        
        
        AudioManager.Instance.PlayAsync(SEType.maou_se_system48, destroyCancellationToken).Forget();
        AudioManager.Instance.ToggleMute(VolumeType.Master);
        await UniTask.WaitForSeconds(1f);
        AudioManager.Instance.ToggleMute(VolumeType.Master);
        
        AudioManager.Instance.StopBGM();
        
        AudioManager.Instance.PlayAsync(SEType.maou_se_system48, destroyCancellationToken).Forget();
        await UniTask.WaitForSeconds(0.5f);
        AudioManager.Instance.StopSE();
    }
}
