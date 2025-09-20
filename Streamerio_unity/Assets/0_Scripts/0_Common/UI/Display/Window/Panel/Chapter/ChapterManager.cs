using System.Collections.Generic;
using System.Threading;
using Alchemy.Inspector;
using Common.UI.Display.Window.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Display.Window.Panel
{
    public class ChapterManager: SingletonBase<ChapterManager>
    {
        [SerializeField, LabelText("最初に開く章")]
        private ChapterType _firstChapterType = ChapterType.Menu;
        [SerializeField, LabelText("章パネル")]
        private　SerializeDictionary<ChapterType, ChapterPanelPresenter> _chapterDict;
        [SerializeField, LabelText("章パネルの親")]
        private Transform _chapterParent;
        
        private Dictionary<ChapterType, ChapterPanelPresenter> _existingChapterDict;
        
        private Stack<ChapterType> _chapterStack;

        private BookWindowAnimation _bookWindowAnimation;
        
        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="bookWindowAnimation"></param>
        public void Initialize(BookWindowAnimation bookWindowAnimation)
        {
            _existingChapterDict = new Dictionary<ChapterType, ChapterPanelPresenter>();
            
            _bookWindowAnimation = bookWindowAnimation;
            
            _chapterStack = new Stack<ChapterType>();
        }

        /// <summary>
        /// チャプターパネルをアニメーションで開く
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ct"></param>
        public async UniTask OpenChapterAsync(ChapterType type, CancellationToken ct)
        {
            if (_chapterStack.Count > 0)
            {
                var currentChapter = _chapterStack.Pop();
                await GetChapter(currentChapter).HideAsync(ct);
                _chapterStack.Push(currentChapter);
            }
            
            _chapterStack.Push(type);
            await GetChapter(type).ShowAsync(ct);
        }
        
        /// <summary>
        /// チャプターパネルを開く
        /// </summary>
        /// <param name="type"></param>
        public void OpenChapter(ChapterType type)
        {
            if (_chapterStack.Count > 0)
            {
                var currentChapter = _chapterStack.Pop();
                GetChapter(currentChapter).Hide();
                _chapterStack.Push(currentChapter);
            }
            
            _chapterStack.Push(type);
            GetChapter(type).Show();
        }
        
        /// <summary>
        /// 最初のチャプターパネルをアニメーションで開く
        /// </summary>
        /// <param name="ct"></param>
        public async UniTask OpenFirstChapterAsync(CancellationToken ct)
        {
            await OpenChapterAsync(_firstChapterType, ct);
        }
        
        /// <summary>
        /// 最初のチャプターパネルを開く
        /// </summary>
        public void OpenFirstChapter()
        {
            OpenChapter(_firstChapterType);
        }
        
        /// <summary>
        /// チャプターパネルを閉じる
        /// </summary>
        /// <param name="ct"></param>
        /// <returns>全てのパネルを閉じたか</returns>
        public async UniTask<bool> CloseChapterAsync(CancellationToken ct)
        {
            if(_chapterStack.Count == 0)
            {
                return true;
            }
            
            var currentChapterType = _chapterStack.Pop();
            await GetChapter(currentChapterType).HideAsync(ct);
            
            if(_chapterStack.TryPop(out var nextChapterType))
            {
                await OpenChapterAsync(nextChapterType, ct);
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// チャプターパネルを閉じる
        /// </summary>
        public bool CloseChapter()
        {
            if(_chapterStack.Count == 0)
            {
                return true;
            }
            
            var currentChapterType = _chapterStack.Pop(); 
            GetChapter(currentChapterType).Hide();
            
            if(_chapterStack.TryPop(out var nextChapterType))
            {
                OpenChapter(nextChapterType);
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// チャプターパネルを取得
        /// </summary>
        /// <param name="chapterType"></param>
        /// <returns></returns>
        private ChapterPanelPresenter GetChapter(ChapterType chapterType)
        {
            if (_existingChapterDict.TryGetValue(chapterType, out var chapter))
            {
                return chapter;
            }
            
            var newChapter = CreateChapter(chapterType);
            
            
            return newChapter;
        }
        
        /// <summary>
        /// 新しいチャプターパネルを作成
        /// </summary>
        /// <param name="chapterType"></param>
        private ChapterPanelPresenter CreateChapter(ChapterType chapterType)
        {
            var newChapter = Instantiate(_chapterDict[chapterType], _chapterParent);
            newChapter.Initialize(_bookWindowAnimation);
         
            _existingChapterDict.Add(chapterType, newChapter);
            
            return newChapter;
        }
    }
}