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
    public abstract class WindowPresenterBase<TView>: DisplayPresenterBase<TView>
        where TView: WindowViewBase
    {
        private ChapterType _currentChapterType;
        [SerializeField, LabelText("最初に開く章")]
        private ChapterType _firstChapterType = ChapterType.Menu;
        
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            ClickGuard.Instance.Guard(true);
            View.SetInteractable(true);
            await View.ShowAsync(ct);
            await GetChapter(_firstChapterType).ShowAsync(ct);
            ClickGuard.Instance.Guard(false);
        }

        public override void Show()
        {
            base.Show();
            GetChapter(_firstChapterType).Show();
        }

        public override async UniTask HideAsync(CancellationToken ct)
        {
            ClickGuard.Instance.Guard(true);
            if(_currentChapterType != ChapterType.None)
                await GetChapter(_currentChapterType).HideAsync(ct);
            await View.HideAsync(ct);
            View.SetInteractable(false);
            ClickGuard.Instance.Guard(false);
        }

        public override void Hide()
        {
            base.Hide();
            if(_currentChapterType != ChapterType.None)
                GetChapter(_currentChapterType).Hide();
        }
        
        /// <summary>
        /// チャプターを取得
        /// </summary>
        /// <param name="chapterType"></param>
        /// <returns></returns>
        private ChapterPanelPresenter GetChapter(ChapterType chapterType)
        {
            if (View.ExisitingChapterDict.TryGetValue(chapterType, out var chapter))
            {
                return chapter;
            }
            
            var newChapter = View.CreateChapter(chapterType);
            
            newChapter.CloseEvent
                .SubscribeAwait(async (chapter, ct) =>
                {
                    if (chapter == ChapterType.None)
                    {
                        await HideAsync(ct);
                    }
                    else
                    {
                        ClickGuard.Instance.Guard(true);
                        await GetChapter(chapter).HideAsync(ct);
                        await GetChapter(chapter).ShowAsync(ct);
                        ClickGuard.Instance.Guard(false);   
                    }
                }).RegisterTo(destroyCancellationToken);
            
            return newChapter;
        }
    }
}