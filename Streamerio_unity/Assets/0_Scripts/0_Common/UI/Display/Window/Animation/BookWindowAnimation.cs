using System.Threading;
using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Display.Window.Animation
{
    public class BookWindowAnimation: MonoBehaviour
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
        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
            
            _defaultBookPosY = _bookWindowRectTransform ? _bookWindowRectTransform.anchoredPosition.y : 0f;
            _defaultBookHeight = _bookWindowRectTransform ? _bookWindowRectTransform.rect.height : 0f;
        }
#endif

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize()
        {
            _lct = new LinkedCancellationToken(destroyCancellationToken);
        }

        /// <summary>
        /// 左ページをめくるアニメーションを再生
        /// </summary>
        /// <param name="ct"></param>
        public async UniTask PlayTurnLeftAsync(CancellationToken ct)
        {
            await PlayTurnAsync(_turnPageLeftTrigger, ct);
        }
        
        /// <summary>
        /// 右ページをめくるアニメーションを再生
        /// </summary>
        /// <param name="ct"></param>
        public async UniTask PlayTurnRightAsync(CancellationToken ct)
        {
            await PlayTurnAsync(_turnPageRightTrigger, ct);
        }

        /// <summary>
        /// ページをめくるアニメーションを再生
        /// </summary>
        /// <param name="triggerName"></param>
        /// <param name="ct"></param>
        private async UniTask PlayTurnAsync(string triggerName, CancellationToken ct)
        {
            _animator.SetTrigger(triggerName);
            await UniTask.WaitWhile(() => _animator.GetCurrentAnimatorStateInfo(0).IsName(_idoleStateName), cancellationToken: _lct.GetCancellationToken(ct));
        }
        
        /// <summary>
        /// ページをめくっている途中のトランスフォームに変更
        /// </summary>
        public void SetTurningBookTransform()
        {
            SetBookTransform(_turningBookPosY, _turningBookHeight);
        }

        /// <summary>
        /// 本のトランスフォームをリセット
        /// </summary>
        public void ResetBookTransform()
        {
            SetBookTransform(_defaultBookPosY, _defaultBookHeight);
        }

        /// <summary>
        /// トランスフォーム設定
        /// </summary>
        /// <param name="posY"></param>
        /// <param name="height"></param>
        private void SetBookTransform(float posY, float height)
        {
            _bookWindowRectTransform.anchoredPosition = new Vector2(_bookWindowRectTransform.anchoredPosition.x, posY);
            _bookWindowRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
