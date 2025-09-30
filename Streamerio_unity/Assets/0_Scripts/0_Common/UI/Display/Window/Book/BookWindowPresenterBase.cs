using System.Threading;
using Common.UI.Display.Window.Panel;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Common.UI.Display.Window.Book
{
    /// <summary>
    /// 共通のウィンドウのプレゼンター基底クラス。
    /// - 最初に開くチャプターを継承先で定義する
    /// - Show/Hide 時に ChapterManager を通してチャプターの開閉を制御する
    /// </summary>
    [RequireComponent(typeof(CommonWindowView))]
    public abstract class BookWindowPresenterBase : DisplayPresenterBase<CommonWindowView>
    {
        public override void Initialize()
        {
            ChapterManager.Instance.Initialize(CommonView.BookAnimation);
            base.Initialize();
        }
        
        protected override void Bind()
        {
            base.Bind();

            ChapterManager.Instance.IsBusyProp
                .DistinctUntilChanged()
                .SkipWhile(isBusy => isBusy)
                .Where(isBusy => !isBusy)
                .SubscribeAwait(async (_, ct) =>
                {
                    await HideAsync(ct);
                }).RegisterTo(destroyCancellationToken);
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            CommonView.SetInteractable(false);
            await ChapterManager.Instance.CloseTopAsync(ct);
            await base.HideAsync(ct);
        }

        public override void Hide()
        {
            base.Hide();
            ChapterManager.Instance.CloseTop();
        }
    }
}