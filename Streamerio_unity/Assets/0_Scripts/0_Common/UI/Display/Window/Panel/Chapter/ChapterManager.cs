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
        
        private ChapterType _currentChapterType;

        private BookWindowAnimation _bookWindowAnimation;
        
        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="bookWindowAnimation"></param>
        public void Initialize(BookWindowAnimation bookWindowAnimation)
        {
            _existingChapterDict = new Dictionary<ChapterType, ChapterPanelPresenter>();
            
            _bookWindowAnimation = bookWindowAnimation;
        }

        /// <summary>
        /// チャプターパネルをアニメーションで開く
        /// </summary>
        /// <param name="type"></param>
        public async UniTask OpenChapterAsync(ChapterType type, CancellationToken ct)
        {
            if(_currentChapterType != ChapterType.None)
                await GetChapter(_currentChapterType).HideAsync(ct);
            
            _currentChapterType = type;
            await GetChapter(type).ShowAsync(ct);
        }
        
        /// <summary>
        /// チャプターパネルを開く
        /// </summary>
        /// <param name="type"></param>
        public void OpenChapter(ChapterType type)
        {
            if(_currentChapterType != ChapterType.None)
                GetChapter(_currentChapterType).Hide();
            
            _currentChapterType = type;
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
        public async UniTask CloseChapterAsync(CancellationToken ct)
        {
            if(_currentChapterType != ChapterType.None)
                await GetChapter(_currentChapterType).HideAsync(ct);
            
            _currentChapterType = ChapterType.None;
        }
        
        /// <summary>
        /// チャプターパネルを閉じる
        /// </summary>
        public void CloseChapter()
        {
            if(_currentChapterType != ChapterType.None)
                GetChapter(_currentChapterType).Hide();
            
            _currentChapterType = ChapterType.None;
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