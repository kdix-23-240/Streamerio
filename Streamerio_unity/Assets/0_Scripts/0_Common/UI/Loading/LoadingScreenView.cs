using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI.Loading
{
    /// <summary>
    /// ローディング画面の見た目
    /// </summary>
    public class LoadingScreenView: UIBehaviourBase
    {
        [SerializeField, ReadOnly]
        private Image _image;
        
        private Material _irisOutMaterial;
        
        [Header("アニメーション")]
        [SerializeField, LabelText("ローディング入りのアニメーション")]
        private IrisAnimationComponentParam _loadingInAnimationParam;
        [SerializeField, LabelText("ローディング出のアニメーション")]
        private IrisAnimationComponentParam _loadingOutAnimationParam;
        
        [SerializeField, LabelText("タイトルからローディングへのアニメーション")]
        private IrisAnimationComponentParam _titleToLoadingAnimationParam;
        
        [SerializeField, LabelText("ローディングからインゲームへのアニメーション")]
        private IrisAnimationComponentParam _loadingToInGameAnimationParam;
        
        [SerializeField]
        private IrisAnimationComponentParam _cheiceIrisAnimationParam = new ()
        {
            Duration = 1.5f,
            Ease = Ease.Linear,
            MinRadius = 0f,
            MaxRadius = 2f,
        };

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
        
        public override void Initialize()
        {
            base.Initialize();
            _irisOutMaterial = _image.material;
            
            _loadingInAnimation = new IrisInAnimationComponent(_irisOutMaterial, _loadingInAnimationParam);
            _loadingOutAnimation = new IrisOutAnimationComponent(_irisOutMaterial, _loadingOutAnimationParam);
            
            _titleToLoadingAnimation = new IrisInAnimationComponent(_irisOutMaterial, _titleToLoadingAnimationParam);
            _loadingToInGameAnimation = new IrisOutAnimationComponent(_irisOutMaterial, _loadingToInGameAnimationParam);
        }
        
        /// <summary>
        /// 表示アニメーション
        /// </summary>
        public async UniTask ShowAsync(CancellationToken ct)
        {
            await _loadingInAnimation.PlayAsync(ct);
        }
        
        /// <summary>
        /// アニメーションで表示
        /// </summary>
        /// <param name="ct"></param>
        public async UniTask ShowAsync(Vector3 centerCirclePosition, CancellationToken ct)
        {
            var centerCircle = Camera.main.WorldToViewportPoint(centerCirclePosition);
            _cheiceIrisAnimationParam.Center = centerCircle;
            
            var irisInAnimation = new IrisInAnimationComponent(_irisOutMaterial, _cheiceIrisAnimationParam);
            await irisInAnimation.PlayAsync(ct);
            SetInteractable(false);
        }
        
        /// <summary>
        /// 表示
        /// </summary>
        public void Show()
        {
            _irisOutMaterial.SetFloat(_loadingInAnimationParam.RadiusPropertyName, _loadingInAnimationParam.MinRadius);
        }
        
        /// <summary>
        /// 非表示アニメーション
        /// </summary>
        public async UniTask HideAsync(CancellationToken ct)
        {
            await _loadingOutAnimation.PlayAsync(ct);
        }
        
        /// <summary>
        /// タイトルからローディングへのアニメーション
        /// </summary>
        /// <param name="ct"></param>
        public async UniTask TitleToLoadingAsync(CancellationToken ct)
        {
            await _titleToLoadingAnimation.PlayAsync(ct);
        }
        
        /// <summary>
        /// ローディングからインゲームへのアニメーション
        /// </summary>
        /// <param name="ct"></param>
        public async UniTask LoadingToInGameAsync(CancellationToken ct)
        {
            await _loadingToInGameAnimation.PlayAsync(ct);
        }
    }
}