using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.UI.Display
{
    /// <summary>
    /// Display の開閉状態をスタックで管理する共通マネージャ。
    /// - 最前面の Display を常に1つに保つ（開くと前の Display は自動で隠す）
    /// - 「開く」だけ / 「閉じられるまで待つ」パターンを使い分け可能
    /// - Display 取得は IDisplayService に委譲
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

        /// <summary>Display 生成・初期化・キャッシュ提供サービス</summary>
        private IDisplayService _displayService;

        /// <summary>現在開いている Display のスタック（後入れ先出し）</summary>
        private Stack<IDisplay> _displayStack;

        /// <summary>
        /// 初期化
        /// </summary>
        public virtual void Initialize()
        {
            _displayService = InstanceDisplayService(_displayRepository, _parent);
            _displayStack = new Stack<IDisplay>();
        }

        /// <summary>
        /// 具象クラスで DisplayService を用意する
        /// </summary>
        protected abstract IDisplayService InstanceDisplayService(TSO repository, Transform parent);

        // -----------------------------
        // 開く系
        // -----------------------------

        /// <summary>
        /// 指定 Display をアニメーションで開く（前面に出す）。前面に居た Display は非表示にする。
        /// 待機はしない（すぐに制御を返す）。
        /// </summary>
        public virtual async UniTask<TDisplay> OpenDisplayAsync<TDisplay>(CancellationToken ct)
            where TDisplay : UIBehaviour, IDisplay
        {
            // 現在前面の Display があれば隠す
            if (TryPeek(out var current))
                await current.HideAsync(ct);

            // 新たに開く
            var display = _displayService.GetDisplay<TDisplay>();
            _displayStack.Push(display);

            await display.ShowAsync(ct);
            return display;
        }

        /// <summary>
        /// 指定 Display を開き、**その Display が閉じられるまで待機**する。
        /// </summary>
        public virtual async UniTask OpenAndWaitCloseAsync<TDisplay>(CancellationToken ct)
            where TDisplay : UIBehaviour, IDisplay
        {
            var display = await OpenDisplayAsync<TDisplay>(ct);

            // 「閉じられるまで待つ」… IsShow が false になるのを待機
            await UniTask.WaitWhile(() => display.IsShow, cancellationToken: ct);
        }

        // -----------------------------
        // 閉じる系
        // -----------------------------

        /// <summary>
        /// 前面（直前に開いた）Display をアニメーションで閉じ、次に下にある Display を再度表示する（待機しない）。
        /// </summary>
        public virtual async UniTask CloseTopAsync(CancellationToken ct)
        {
            if (!TryPop(out var current))
                return;

            await current.HideAsync(ct);

            // 下にある Display を再表示（存在する場合）
            await ShowTopAsync(ct);
        }

        /// <summary>
        /// すべての Display を即座に閉じ、スタックをクリアする（非同期待機なし）。
        /// </summary>
        public void CloseAll()
        {
            while (_displayStack.Count > 0)
            {
                var d = _displayStack.Pop();
                d.Hide();
            }
        }

        // -----------------------------
        // 内部ヘルパー
        // -----------------------------

        /// <summary>
        /// スタック先頭の Display をアニメーションで表示（待機）する。
        /// </summary>
        private async UniTask ShowTopAsync(CancellationToken ct)
        {
            if (!TryPeek(out var next))
                return;

            await next.ShowAsync(ct);
        }

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

        private bool TryPop(out IDisplay display)
        {
            if (_displayStack.Count > 0)
            {
                display = _displayStack.Pop();
                return true;
            }
            display = null;
            return false;
        }
    }
}
