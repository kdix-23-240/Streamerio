using System.Threading;
using Common.UI.Display.Window.Panel;
using Common.UI.Guard;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Display.Window
{
    /// <summary>
    /// 共通のウィンドウの繋ぎ役
    /// </summary>
    [RequireComponent(typeof(WindowView))]
    public class WindowPresenter: DisplayPresenterBase<WindowView>
    {
        public override void Initialize()
        {
            base.Initialize();
            ChapterManager.Instance.Initialize(View.BookWindowAnimation);
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            ClickGuard.Instance.Guard(true);
            View.SetInteractable(true);
            await View.ShowAsync(ct);
            await ChapterManager.Instance.OpenFirstChapterAsync(ct);
            ClickGuard.Instance.Guard(false);
        }

        public override void Show()
        {
            base.Show();
            ChapterManager.Instance.OpenFirstChapter();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            ClickGuard.Instance.Guard(true);
            await ChapterManager.Instance.CloseChapterAsync(ct);
            await View.HideAsync(ct);
            View.SetInteractable(false);
            ClickGuard.Instance.Guard(false);
        }

        public override void Hide()
        {
            base.Hide();
            ChapterManager.Instance.CloseChapter();
        }
    }
}