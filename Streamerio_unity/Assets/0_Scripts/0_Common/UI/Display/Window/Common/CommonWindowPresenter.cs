using System.Threading;
using Alchemy.Inspector;
using Common.UI.Display.Window.Panel;
using Common.UI.Guard;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Common.UI.Display.Window
{
    /// <summary>
    /// 共通のウィンドウの繋ぎ役
    /// </summary>
    [RequireComponent(typeof(CommonWindowView))]
    public class CommonWindowPresenter: DisplayPresenterBase<CommonWindowView>
    {
        private ChapterType _currentChapterType;
        [SerializeField, LabelText("最初に開く章")]
        private ChapterType _firstChapterType = ChapterType.Menu;

        protected override void Bind()
        {
            foreach (var keyValuePair in View.ChapterDict)
            {
                keyValuePair.Value.CloseEvent
                    .SubscribeAwait(async (chapter, ct) =>
                    {
                        if (chapter == ChapterType.None)
                        {
                            await HideAsync(ct);
                        }
                        else
                        {
                            ClickGuard.Instance.Guard(true);
                            await View.ChapterDict[chapter].HideAsync(ct);
                            await View.ChapterDict[chapter].ShowAsync(ct);
                            ClickGuard.Instance.Guard(false);   
                        }
                    }).RegisterTo(destroyCancellationToken);
            }
        }
        
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            ClickGuard.Instance.Guard(true);
            View.SetInteractable(true);
            await View.ShowAsync(ct);
            await View.ChapterDict[_firstChapterType].ShowAsync(ct);
            ClickGuard.Instance.Guard(false);
        }

        public override void Show()
        {
            base.Show();
            View.ChapterDict[_firstChapterType].Show();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            ClickGuard.Instance.Guard(true);
            if(_currentChapterType != ChapterType.None)
                await View.ChapterDict[_currentChapterType].HideAsync(ct);
            await View.HideAsync(ct);
            View.SetInteractable(false);
            ClickGuard.Instance.Guard(false);
        }

        public override void Hide()
        {
            base.Hide();
            View.ChapterDict[_firstChapterType].Hide();
        }
    }
}