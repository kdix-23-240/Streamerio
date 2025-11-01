using System.Threading;
using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Display.Window.Book
{
    /// <summary>
    /// 本型ウィンドウのページめくり演出を制御するコンポーネント。
    /// - Animator を用いてページめくりアニメーションを再生
    /// - ページめくり中は RectTransform の位置やサイズを調整して演出を強化
    /// - 非同期メソッドでアニメーションの終了を待機可能
    /// </summary>
    public class BookWindowAnimation : MonoBehaviour
    {
        [SerializeField, ReadOnly]
        private Animator _animator;

        [SerializeField, LabelText("本のウィンドウのRectTransform")]
        private RectTransform _bookWindowRectTransform;

        [Header("アニメーションのステート名")]
        [SerializeField, LabelText("アイドルステート名")]
        private string _idoleStateName = "Idle";

        [Header("アニメーションのトリガー名")]
        [SerializeField, LabelText("左ページをめくるトリガー名")]
        private string _turnPageLeftTrigger = "TurnPageLeft";
        [SerializeField, LabelText("右ページをめくるトリガー名")]
        private string _turnPageRightTrigger = "TurnPageRight";

        [Header("本をめくる時の位置調整")]
        [SerializeField, LabelText("通常の本の位置"), ReadOnly]
        private float _defaultBookPosY;
        [SerializeField, LabelText("めくっている途中の本の位置")]
        private float _turningBookPosY;

        [Header("本をめくる時のサイズ調整")]
        [SerializeField, LabelText("通常の本の高さ"), ReadOnly]
        private float _defaultBookHeight;
        [SerializeField, LabelText("めくっている途中の本の高さ")]
        private float _turningBookHeight;

        private LinkedCancellationToken _lct;

#if UNITY_EDITOR
        /// <summary>
        /// エディタ上で変更があったときに参照を補完＆デフォルト値をキャッシュ
        /// </summary>
        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();

            _defaultBookPosY = _bookWindowRectTransform ? _bookWindowRectTransform.anchoredPosition.y : 0f;
            _defaultBookHeight = _bookWindowRectTransform ? _bookWindowRectTransform.rect.height : 0f;
        }
#endif

        /// <summary>
        /// 初期化。
        /// - キャンセルトークンを構築して非同期アニメーション制御に利用
        /// </summary>
        public void Initialize()
        {
            _lct = new LinkedCancellationToken(destroyCancellationToken);
        }

        /// <summary>
        /// 左ページをめくるアニメーションを再生（完了まで待機）。
        /// </summary>
        public async UniTask PlayTurnLeftAsync(CancellationToken ct)
        {
            await PlayTurnAsync(_turnPageLeftTrigger, ct);
        }

        /// <summary>
        /// 右ページをめくるアニメーションを再生（完了まで待機）。
        /// </summary>
        public async UniTask PlayTurnRightAsync(CancellationToken ct)
        {
            await PlayTurnAsync(_turnPageRightTrigger, ct);
        }

        /// <summary>
        /// 共通：ページめくりアニメーションを再生。
        /// - 指定トリガーを発火
        /// - Idle ステートを抜けるまで待機
        /// </summary>
        private async UniTask PlayTurnAsync(string triggerName, CancellationToken ct)
        {
            _animator.SetTrigger(triggerName);

            // Idle 以外のアニメーションが再生されるまで待機
            await UniTask.WaitWhile(
                () => _animator.GetCurrentAnimatorStateInfo(0).IsName(_idoleStateName),
                cancellationToken: _lct.GetCancellationToken(ct));
        }

        /// <summary>
        /// ページめくり中の見た目に変更。
        /// - RectTransform の位置と高さを調整
        /// </summary>
        public void SetTurningBookTransform()
        {
            SetBookTransform(_turningBookPosY, _turningBookHeight);
        }

        /// <summary>
        /// 本の見た目をデフォルト状態に戻す。
        /// </summary>
        public void ResetBookTransform()
        {
            SetBookTransform(_defaultBookPosY, _defaultBookHeight);
        }

        /// <summary>
        /// RectTransform の位置と高さをまとめて変更。
        /// </summary>
        private void SetBookTransform(float posY, float height)
        {
            _bookWindowRectTransform.anchoredPosition =
                new Vector2(_bookWindowRectTransform.anchoredPosition.x, posY);

            _bookWindowRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}