// モジュール概要:
// 複数 CanvasGroup を束ね、フェード演出付きの表示/非表示を統一制御するグループコンポーネント。
// 依存関係: FadePartsAnimationComponentParamSO で演出パラメータを取得し、FadePartsAnimationComponent が DOTween 経由でアニメーションを再生する。
// 使用例: ダイアログや画面内セクションのルートにアタッチし、Presenter から ShowAsync/HideAsync を呼び出して段階的に表示する。

using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer.Unity;

namespace Common.UI.Part.Group
{
    /// <summary>
    /// 【目的】複数の UI パーツを統一挙動でフェード制御できるようにする。
    /// 【理由】パーツごとに個別演出を組むと同期ズレが発生しやすいため、まとめて制御して一貫した画面演出を維持する。
    /// </summary>
    public class CommonUIPartGroup : UIBehaviour, ICommonUIPartGroup, IInitializable
    {
        /// <summary>
        /// 【目的】演出対象となる CanvasGroup 群を指定する。
        /// 【理由】Inspector から順序付きで設定し、フェードの遅延演出に利用するため。
        /// </summary>
        [SerializeField, LabelText("アニメーションさせるオブジェクト群")]
        private CanvasGroup[] _uiParts =new CanvasGroup[]{};
        
        [Header("アニメーション")]
        /// <summary>
        /// 【目的】表示時に使用するフェード演出パラメータを保持する。
        /// 【理由】演出速度や遅延を ScriptableObject で管理し、デザイナーが差し替えやすくするため。
        /// </summary>
        [SerializeField, LabelText("表示アニメーション (フェード + 遅延)")]
        private FadePartsAnimationComponentParamSO _showFadeAnimationParam;
        
        /// <summary>
        /// 【目的】非表示時に使用するフェード演出パラメータを保持する。
        /// 【理由】表示とは異なるアルファ値・遅延を設定できるようにし、撤退時の印象を調整する。
        /// </summary>
        [SerializeField, LabelText("非表示アニメーション (フェード + 遅延)")]
        private FadePartsAnimationComponentParamSO _hideFadeAnimationParam;  
        
        /// <summary>
        /// 【目的】表示演出を再生するコンポーネントを初期化後キャッシュする。
        /// 【理由】毎回コンポーネントを生成すると GC が発生し、連続表示時のフレーム落ちにつながるため。
        /// </summary>
        private FadePartsAnimationComponent _showAnimations;
        /// <summary>
        /// 【目的】非表示演出を再生するコンポーネントをキャッシュする。
        /// 【理由】Hide 操作を頻繁に行う際にインスタンス生成コストを抑えるため。
        /// </summary>
        private FadePartsAnimationComponent _hideAnimation;

        /// <summary>
        /// 【目的】表示/非表示アニメーションを初期化し、再利用可能な状態へ整える。
        /// 【理由】演出開始時にパラメータからコンポーネントを構築していると初回表示が遅延するため。
        /// </summary>
        public void Initialize()
        {
            _showAnimations = new FadePartsAnimationComponent(_uiParts, _showFadeAnimationParam);
            _hideAnimation  = new FadePartsAnimationComponent(_uiParts, _hideFadeAnimationParam);
        }
        
        /// <summary>
        /// 【目的】フェードアニメーションで順番に UI パーツを表示する。
        /// 【理由】視線誘導や段階的な演出を組みやすくし、唐突な表示を避けるため。
        /// </summary>
        /// <param name="ct">【用途】演出途中で表示処理を中断したい場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】全パーツの表示演出が完了したことを示す UniTask。</returns>
        public async UniTask ShowAsync(CancellationToken ct)
        {
            await _showAnimations.PlayAsync(ct);
        }

        /// <summary>
        /// 【目的】アニメーションをスキップして即座に表示状態へ切り替える。
        /// 【理由】デバッグ時や演出を省略した再表示など、待機時間を許容できない場面に対応する。
        /// </summary>
        public void Show()
        {
            SetAlphaParts(_showFadeAnimationParam.Alpha);
        }
        
        /// <summary>
        /// 【目的】フェードアニメーションで順番に UI パーツを非表示にする。
        /// 【理由】画面を落ち着かせながら閉じ、急な暗転による違和感を減らすため。
        /// </summary>
        /// <param name="ct">【用途】演出途中で非表示処理を中断したい場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】全パーツの非表示演出が完了したことを示す UniTask。</returns>
        public async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
        }
        
        /// <summary>
        /// 【目的】アニメーションをスキップして即座に非表示へ切り替える。
        /// 【理由】遷移を急ぐケースや演出をスキップしたリセット時に待機を発生させないため。
        /// </summary>
        public void Hide()
        {
            SetAlphaParts(_hideFadeAnimationParam.Alpha);
        }
        
        /// <summary>
        /// 【目的】登録されているすべての UI パーツに同一アルファ値を適用する。
        /// 【理由】即時表示/非表示時に最終アルファへ整えることで、演出スキップ後も整合性を保つ。
        /// </summary>
        /// <param name="alpha">【用途】適用したいアルファ値。</param>
        private void SetAlphaParts(float alpha)
        {
            foreach (var part in _uiParts)
            {
                part.alpha = alpha;
            }
        }
    }
    
    public interface ICommonUIPartGroup
    {
        UniTask ShowAsync(CancellationToken ct);
        void Show();
        UniTask HideAsync(CancellationToken ct);
        void Hide();
    }
}
