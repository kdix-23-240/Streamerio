using UnityEngine;
using UnityEngine.Pool;

namespace Common.Audio
{
    /// <summary>
    /// オーディオソースのプール管理クラス。
    /// - UnityEngine.Pool.ObjectPool を利用して Source を効率的に再利用する
    /// - AudioManager 経由で BGM/SE 再生に使われる
    /// - Source は再生終了時に自動で Release されるため、明示的な返却処理は不要
    /// </summary>
    public class AudioSourcePool
    {
        private readonly Source _prefab;         // 生成元となるプレハブ
        private readonly Transform _parent;      // 親 Transform（Hierarchy整理用）
        private readonly ObjectPool<Source> _sourcePool;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="source">生成元となる Source プレハブ</param>
        /// <param name="parent">生成時の親 Transform</param>
        /// <param name="capacity">プールの初期容量</param>
        public AudioSourcePool(Source source, Transform parent, int capacity)
        {
            _prefab = source;
            _parent = parent;

            _sourcePool = new ObjectPool<Source>(
                createFunc: OnCreateSource,
                actionOnGet: OnGetSource,
                actionOnRelease: OnReleaseSource,
                actionOnDestroy: OnDestroySource,
                defaultCapacity: capacity
            );
        }

        /// <summary>
        /// プールからオーディオソースを取得。
        /// - 利用後は Source 側で自動的に Release される
        /// </summary>
        public Source GetSource()
        {
            return _sourcePool.Get();
        }

        /// <summary>
        /// プールをクリアして全インスタンスを破棄。
        /// </summary>
        public void Clear()
        {
            _sourcePool.Clear();
        }

        // =============================
        // ObjectPool 用コールバック群
        // =============================

        /// <summary>
        /// 新しいインスタンス生成時の処理。
        /// </summary>
        private Source OnCreateSource()
        {
            return Object.Instantiate(_prefab, _parent);
        }

        /// <summary>
        /// プールから取得された時の処理。
        /// - Dispose 時に Release されるようにコールバックを設定
        /// </summary>
        private void OnGetSource(Source source)
        {
            source.Initialize(() => _sourcePool.Release(source));
        }

        /// <summary>
        /// プールに返却された時の処理。
        /// - 今回は特に何もしない（必要なら状態リセット処理を入れる）
        /// </summary>
        private void OnReleaseSource(Source source)
        {
            
        }

        /// <summary>
        /// プールから削除される時の処理。
        /// - Dispose を呼んでから GameObject を破棄
        /// </summary>
        private void OnDestroySource(Source source)
        {
            source.Dispose();
            Object.Destroy(source.gameObject);
        }
    }
}