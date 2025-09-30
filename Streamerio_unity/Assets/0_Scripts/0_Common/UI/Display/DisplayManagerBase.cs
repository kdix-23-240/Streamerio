using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Display
{
    /// <summary>
    /// Display の開閉状態をスタックで管理する共通マネージャ。
    /// - 最前面の Display を常に1つに保つ（開くと前の Display は自動で隠す）
    /// - 「開くだけ」/「閉じられるまで待つ」両方の使い方が可能
    /// - Display の生成は IDisplayService に委譲
    /// - IsBusy は「開いている Display があるかどうか」を表す
    /// </summary>
    /// <typeparam name="TSO">Display リポジトリ（ScriptableObject）型</typeparam>
    /// <typeparam name="TManager">シングルトン自身の型</typeparam>
    public abstract class DisplayManagerBase<TSO, TManager> : SingletonBase<TManager>
        where TSO : ScriptableObject, IDisplayRepository
        where TManager : DisplayManagerBase<TSO, TManager>
    {
        [SerializeField, LabelText("UIのリポジトリ")]
        private TSO _displayRepository;

        [SerializeField, LabelText("UIの親")]
        private Transform _parent;

        /// <summary>Display の生成・初期化・キャッシュを担当するサービス</summary>
        private IDisplayService _displayService;

        /// <summary>現在開いている Display を管理するスタック（後入れ先出し）</summary>
        private Stack<IDisplay> _displayStack;

        /// <summary>現在 UI が開いているかどうかを表すフラグ</summary>
        private ReactiveProperty<bool> _isBusyProp;
        /// <summary>外部公開用の読み取り専用プロパティ</summary>
        public ReadOnlyReactiveProperty<bool> IsBusyProp => _isBusyProp;

        /// <summary>
        /// 初期化処理。
        /// - DisplayService を生成
        /// - スタックを初期化
        /// - IsBusy を false で開始
        /// </summary>
        public virtual void Initialize()
        {
            _displayService = InstanceDisplayService(_displayRepository, _parent);
            _displayStack = new Stack<IDisplay>();
            _isBusyProp = new ReactiveProperty<bool>(false);
        }

        /// <summary>
        /// 具象クラスで DisplayService を提供する
        /// </summary>
        protected abstract IDisplayService InstanceDisplayService(TSO repository, Transform parent);

        // -----------------------------
        // 開く処理
        // -----------------------------

        /// <summary>
        /// 指定した Display をアニメーションで開く。
        /// - 既に前面に Display があれば非表示にする
        /// - 新しい Display をスタックに積む
        /// - IsBusy を true に更新
        /// </summary>
        public virtual async UniTask<TDisplay> OpenDisplayAsync<TDisplay>(CancellationToken ct)
            where TDisplay : UIBehaviour, IDisplay
        {
            _isBusyProp.Value = true;

            if (TryPeek(out var current))
                await current.HideAsync(ct);

            var display = _displayService.GetDisplay<TDisplay>();
            _displayStack.Push(display);
            Debug.Log($"[DisplayManager] Open {typeof(TDisplay).Name}, StackCount: {_displayStack.Count}");

            await display.ShowAsync(ct);
            return display;
        }

        /// <summary>
        /// 指定した Display を開き、閉じられるまで待機する。
        /// - IsShow が false になるまで待つ
        /// </summary>
        public virtual async UniTask OpenAndWaitCloseAsync<TDisplay>(CancellationToken ct)
            where TDisplay : UIBehaviour, IDisplay
        {
            var display = await OpenDisplayAsync<TDisplay>(ct);
            await UniTask.WaitWhile(() => display.IsShow, cancellationToken: ct);
        }

        /// <summary>
        /// 非同期処理を待たずに Display を開く。
        /// </summary>
        public virtual void OpenDisplay<TDisplay>()
            where TDisplay : UIBehaviour, IDisplay
        {
            _isBusyProp.Value = true;

            if (TryPeek(out var current))
                current.Hide();

            GetDisplay<TDisplay>().Show();
        }

        // -----------------------------
        // 閉じる処理
        // -----------------------------

        /// <summary>
        /// 最前面の Display をアニメーションで閉じ、下の Display を再表示する。
        /// - スタックが空になったら IsBusy を false にする
        /// </summary>
        public virtual async UniTask CloseTopAsync(CancellationToken ct)
        {
            if (!TryPop(out var current))
                return;

            Debug.Log($"[DisplayManager] Close {current.GetType().Name}, StackCount: {_displayStack.Count}");
            await current.HideAsync(ct);
            await ShowTopAsync(ct);
        }

        /// <summary>
        /// 最前面の Display を即時閉じ、下の Display を再表示する。
        /// - スタックが空になったら IsBusy を false にする
        /// </summary>
        public virtual void CloseTop()
        {
            if (!TryPop(out var current))
                return;

            current.Hide();
            ShowTop();
        }

        /// <summary>
        /// すべての Display を即時閉じ、スタックをクリア。
        /// - IsBusy を false にする
        /// </summary>
        public void CloseAll()
        {
            while (_displayStack.Count > 0)
            {
                var d = _displayStack.Pop();
                d.Hide();
            }

            _isBusyProp.Value = false;
        }

        // -----------------------------
        // 内部ヘルパー
        // -----------------------------

        /// <summary>
        /// スタックの一番上の Display をアニメーションで再表示する。
        /// </summary>
        private async UniTask ShowTopAsync(CancellationToken ct)
        {
            if (!TryPeek(out var next))
            {
                _isBusyProp.Value = false;
                return;
            }

            Debug.Log($"[DisplayManager] Show {next.GetType().Name}, StackCount: {_displayStack.Count}");
            await next.ShowAsync(ct);
        }

        /// <summary>
        /// スタックの一番上の Display を即時表示する。
        /// </summary>
        private void ShowTop()
        {
            if (!TryPeek(out var next))
            {
                _isBusyProp.Value = false;
                return;
            }

            next.Show();
        }

        /// <summary>
        /// 指定型の Display を取得してスタックに積む。
        /// </summary>
        private IDisplay GetDisplay<TDisplay>()
            where TDisplay : UIBehaviour, IDisplay
        {
            var display = _displayService.GetDisplay<TDisplay>();
            _displayStack.Push(display);
            return display;
        }

        /// <summary>
        /// スタックの先頭を覗き見る（取り出さない）
        /// </summary>
        private bool TryPeek(out IDisplay display)
        {
            if (_displayStack.Count > 0)
            {
                display = _displayStack.Peek();
                return true;
            }

            display = null;
            return false;
        }

        /// <summary>
        /// スタックの先頭を取り出す。
        /// - スタックが空になったら IsBusy を false にする
        /// </summary>
        private bool TryPop(out IDisplay display)
        {
            if (_displayStack.Count > 0)
            {
                display = _displayStack.Pop();

                if (_displayStack.Count == 0)
                    _isBusyProp.Value = false;

                return true;
            }

            display = null;
            return false;
        }
    }
}