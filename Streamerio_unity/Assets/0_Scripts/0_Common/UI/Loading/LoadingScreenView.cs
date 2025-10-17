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
    /// </summary>
    public class LoadingScreenView : UIBehaviourBase, IInitializable
    {
        [SerializeField, ReadOnly]
        private Image _image;

        /// <summary>
        /// 実行時に複製して利用するマテリアル。
        /// （共有マテリアルを直接操作すると他オブジェクトへ影響するため注意）
        /// </summary>
        private Material _irisOutMaterial;

        [Header("アニメーションパラメータ")]
        [SerializeField, LabelText("ローディング入りのアニメーション (外→内)")]
        private IrisAnimationComponentParamSO _loadingInAnimationParam;

        [SerializeField, LabelText("ローディング出のアニメーション (内→外)")]
        private IrisAnimationComponentParamSO _loadingOutAnimationParam;

        [SerializeField, LabelText("タイトル→ローディング (外→内)")]
        private IrisAnimationComponentParamSO _titleToLoadingAnimationParam;

        [SerializeField, LabelText("ローディング→インゲーム (内→外)")]
        private IrisAnimationComponentParamSO _loadingToInGameAnimationParam;

        [SerializeField, LabelText("任意中心 (クリック位置など) からのアニメーション (外→内)")]
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
}