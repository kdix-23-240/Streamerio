using System.Threading;
using Common.Scene;
using Cysharp.Threading.Tasks;

namespace Common.State
{
    public class ToResultState: ChangeSceneState
    {
        public ToResultState() : base(SceneType.ResultScene)
        {
        }
        
        public override async UniTask ExitAsync(CancellationToken ct)
        {
            await WebSocketManager.Instance.GameEnd();
        }
    }
}