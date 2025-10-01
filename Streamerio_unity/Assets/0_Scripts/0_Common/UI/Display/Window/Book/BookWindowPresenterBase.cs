using System.Threading;
using Alchemy.Inspector;
using Common.UI.Display.Window.Panel;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Common.UI.Display.Window.Book
{
    /// <summary>
    /// 共通の「本型ウィンドウ」のプレゼンター基底クラス。
    /// - 「本を開く」ような演出を持つウィンドウの基盤
    /// - 派生クラスで最初に開くチャプターを指定して利用する
    /// - ChapterManager を通じてチャプターを制御
    /// - ChapterManager の状態を監視し、すべてのチャプターが閉じたら自動的にウィンドウも閉じる
    /// </summary>
    [RequireComponent(typeof(BookWindowAnimation))]
    public abstract class BookWindowPresenterBase : WindowPresenterBase
    {
        [SerializeField, ReadOnly]
        private BookWindowAnimation _bookAnimation;

#if UNITY_EDITOR
        /// <summary>
        /// Inspector 上で未設定なら自動補完する。
        /// - BookWindowAnimation を必ず参照するようにする
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            _bookAnimation ??= GetComponent<BookWindowAnimation>();
        }
#endif

        /// <summary>
        /// 初期化処理。
        /// - BookWindowAnimation の初期化
        /// - ChapterManager を初期化し、本のアニメーションを連携
        /// - 基底の WindowPresenterBase の初期化も実行
        /// </summary>
        public override void Initialize()
        {
            _bookAnimation.Initialize();
            ChapterManager.Instance.Initialize(_bookAnimation);
            base.Initialize();
        }

        /// <summary>
        /// イベント購読の設定。
        /// - ChapterManager の IsBusy を監視し、
        ///   すべてのチャプターが閉じて Busy が false になった瞬間に
        ///   自身のウィンドウを閉じるようにする。
        /// </summary>
        protected override void Bind()
        {
            base.Bind();

            ChapterManager.Instance.IsBusyProp
                .DistinctUntilChanged()   // 状態が変わった時だけ反応
                .SkipWhile(isBusy => isBusy) // Busy 中はスキップ
                .Where(isBusy => !isBusy) // false (チャプター全閉じ) になった時だけ実行
                .SubscribeAwait(async (_, ct) =>
                {
                    // チャプターが全て閉じた → ウィンドウも閉じる
                    await HideAsync(ct);
                })
                .RegisterTo(destroyCancellationToken);
        }

        /// <summary>
        /// 非表示処理（アニメーションあり）。
        /// - 自身を操作不可にする
        /// - ChapterManager にトップのチャプターを閉じるよう指示
        /// - 最後に基底クラスの HideAsync を実行
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            CommonView.SetInteractable(false);
            await ChapterManager.Instance.CloseTopAsync(ct);
            await base.HideAsync(ct);
        }

        /// <summary>
        /// 非表示処理（即時版）。
        /// - 自身を即座に非表示
        /// - ChapterManager にも即座に閉じるよう指示
        /// </summary>
        public override void Hide()
        {
            base.Hide();
            ChapterManager.Instance.CloseTop();
        }
    }
}
