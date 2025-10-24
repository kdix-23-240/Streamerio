// ============================================================================
// モジュール概要: ローディング画面の View 実装として、Iris 系アニメーションの初期化と演出再生を担う。
// 外部依存: Cysharp.Threading.Tasks、DG.Tweening、UnityEngine.UI、VContainer。
// 使用例: LoadingScreenPresenter が ILoadingScreenView を介して ShowAsync/HideAsync を呼び出し、画面遷移演出を統一する。
// ============================================================================

using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;

namespace Common.UI.Loading
{
    /// <summary>
    /// ローディング画面の View。
    /// - Image に専用マテリアルを割り当て、シェーダプロパティを書き換えてイリス演出を実現
    /// - IrisIn/IrisOut コンポーネントを用いて表示/非表示/シーン遷移をアニメーション制御
    /// <para>
    /// 【理由】Presenter 側のロジックを軽量に保ち、演出関連の責務を View へ切り出すため。
    /// </para>
    /// </summary>
    public class LoadingScreenView : UIBehaviourBase, ILoadingScreenView, IInitializable
    {
        /// <summary>
        /// 【目的】シェーダーマテリアルを適用するターゲット Image を保持する。
        /// </summary>
        [SerializeField, ReadOnly]
        [Tooltip("イリス演出用マテリアルを適用する Image コンポーネント。")]
        private Image _image;

        /// <summary>
        /// 実行時に複製して利用するマテリアル。
        /// （共有マテリアルを直接操作すると他オブジェクトへ影響するため注意）
        /// </summary>
        private Material _irisOutMaterial;

        [Header("アニメーションパラメータ")]
        /// <summary>
        /// 【目的】ローディング突入演出のアセットを保持し、アニメ初期化時に参照できるようにする。
        /// </summary>
        [SerializeField, LabelText("ローディング入りのアニメーション (外→内)")]
        [Tooltip("ロード開始時に画面が閉じる演出用パラメータ。")]
        private IrisAnimationComponentParamSO _loadingInAnimationParam;

        /// <summary>
        /// 【目的】ローディング終了演出のアセットを保持する。
        /// </summary>
        [SerializeField, LabelText("ローディング出のアニメーション (内→外)")]
        [Tooltip("ロード完了時に画面が開く演出用パラメータ。")]
        private IrisAnimationComponentParamSO _loadingOutAnimationParam;

        /// <summary>
        /// 【目的】タイトル画面からローディングへ遷移する際のアニメーション設定。
        /// </summary>
        [SerializeField, LabelText("タイトル→ローディング (外→内)")]
        [Tooltip("タイトル画面から閉じる方向への演出設定。")]
        private IrisAnimationComponentParamSO _titleToLoadingAnimationParam;

        /// <summary>
        /// 【目的】ローディングからゲームプレイへ遷移する際のアニメーション設定。
        /// </summary>
        [SerializeField, LabelText("ローディング→インゲーム (内→外)")]
        [Tooltip("ゲーム開始時に開く方向への演出設定。")]
        private IrisAnimationComponentParamSO _loadingToInGameAnimationParam;

        /// <summary>
        /// 【目的】任意の中心位置からイリス演出を行うときのアセット。
        /// </summary>
        [SerializeField, LabelText("任意中心 (クリック位置など) からのアニメーション (外→内)")]
        [Tooltip("クリック位置などを中心に閉じる演出設定。")]
        private IrisAnimationComponentParamSO _cheiceIrisAnimationParam;

        // 実際のアニメーション制御用コンポーネント
        private IrisInAnimationComponent _loadingInAnimation;
        private IrisOutAnimationComponent _loadingOutAnimation;
        private IrisInAnimationComponent _titleToLoadingAnimation;
        private IrisOutAnimationComponent _loadingToInGameAnimation;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _image ??= GetComponent<Image>();
        }
#endif

        /// <summary>
        /// 初期化。
        /// - Image.material を複製して専用マテリアルを生成
        /// - 各種アニメーションコンポーネントを作成
        /// </summary>
        public void Initialize()
        {
            // 実行時に専用マテリアルを複製
            _irisOutMaterial = new Material(_image.material);
            _image.material = _irisOutMaterial;

            // 各種 Iris コンポーネントを作成
            _loadingInAnimation       = new IrisInAnimationComponent(_irisOutMaterial, _loadingInAnimationParam);
            _loadingOutAnimation      = new IrisOutAnimationComponent(_irisOutMaterial, _loadingOutAnimationParam);
            _titleToLoadingAnimation  = new IrisInAnimationComponent(_irisOutMaterial, _titleToLoadingAnimationParam);
            _loadingToInGameAnimation = new IrisOutAnimationComponent(_irisOutMaterial, _loadingToInGameAnimationParam);
        }

        /// <summary>
        /// アニメーションで表示（外→内へ収束）。
        /// </summary>
        public async UniTask ShowAsync(CancellationToken ct)
        {
            await _loadingInAnimation.PlayAsync(ct);
        }

        /// <summary>
        /// 任意のワールド座標を中心にしてアニメーション表示。
        /// - WorldToViewportPoint で座標を UV に変換して中心に設定
        /// </summary>
        public async UniTask ShowAsync(Vector3 centerCirclePosition, CancellationToken ct)
        {
            var centerCircle = Camera.main.WorldToViewportPoint(centerCirclePosition);
            _cheiceIrisAnimationParam.Center = centerCircle;

            var irisInAnimation = new IrisInAnimationComponent(_irisOutMaterial, _cheiceIrisAnimationParam);
            await irisInAnimation.PlayAsync(ct);

            // アニメーション再生後は UI を操作不可にする
            SetInteractable(false);
        }

        /// <summary>
        /// 即時表示。
        /// - 半径を最小値に設定（閉じた状態）
        /// </summary>
        public void Show()
        {
            _irisOutMaterial.SetFloat(_loadingInAnimationParam.RadiusPropertyName, _loadingInAnimationParam.MinRadius);
        }

        /// <summary>
        /// アニメーションで非表示（内→外へ拡散）。
        /// </summary>
        public async UniTask HideAsync(CancellationToken ct)
        {
            await _loadingOutAnimation.PlayAsync(ct);
        }

        /// <summary>
        /// タイトル → ローディング遷移（外→内）。
        /// </summary>
        public async UniTask TitleToLoadingAsync(CancellationToken ct)
        {
            await _titleToLoadingAnimation.PlayAsync(ct);
        }

        /// <summary>
        /// ローディング → インゲーム遷移（内→外）。
        /// </summary>
        public async UniTask LoadingToInGameAsync(CancellationToken ct)
        {
            await _loadingToInGameAnimation.PlayAsync(ct);
        }
    }
    
    /// <summary>
    /// ローディング画面の View 契約。
    /// <para>
    /// 【理由】Presenter から View 操作を抽象化し、テストや差し替えを容易にするため。
    /// </para>
    /// </summary>
    public interface ILoadingScreenView: ICommonUIBehaviour
    {
        /// <summary>
        /// 【目的】標準アニメーションで表示する。
        /// </summary>
        UniTask ShowAsync(CancellationToken ct);

        /// <summary>
        /// 【目的】任意中心座標を指定して表示する。
        /// </summary>
        UniTask ShowAsync(Vector3 centerCirclePosition, CancellationToken ct);

        /// <summary>
        /// 【目的】演出を介さず即座に表示し、レイアウト調整などに利用する。
        /// </summary>
        void Show();

        /// <summary>
        /// 【目的】標準アニメーションで非表示にする。
        /// </summary>
        UniTask HideAsync(CancellationToken ct);

        /// <summary>
        /// 【目的】タイトル画面からローディングへ移行する演出を再生する。
        /// </summary>
        UniTask TitleToLoadingAsync(CancellationToken ct);

        /// <summary>
        /// 【目的】ローディングからインゲームへ移行する演出を再生する。
        /// </summary>
        UniTask LoadingToInGameAsync(CancellationToken ct);

        /// <summary>
        /// 【目的】UI の操作可否を切り替える。
        /// </summary>
        void SetInteractable(bool interactable);
    }
}
