using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
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

        private IrisInAnimationComponent _loadingInAnimation;
        private IrisOutAnimationComponent _loadingOutAnimation;
        
        private IrisInAnimationComponent _titleToLoadingAnimation;
        
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
        }
        
        /// <summary>
        /// 表示アニメーション
        /// </summary>
        public async UniTask ShowAsync(CancellationToken ct)
        {
            await _loadingInAnimation.PlayAsync(ct);
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
    }
}