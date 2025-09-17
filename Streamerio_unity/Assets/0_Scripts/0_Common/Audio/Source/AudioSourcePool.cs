using UnityEngine;
using UnityEngine.Pool;

namespace Common.Audio
{
    /// <summary>
    /// オーディオソースのプール
    /// </summary>
    public class AudioSourcePool
    {
        private Source _source;
        private Transform _parent;

        private ObjectPool<Source> _sourcePool;

        public AudioSourcePool(Source source, Transform parent, int capacity)
        {
            _source = source;
            _parent = parent;

            _sourcePool = new(
                () => OnCreateSource(),
                (source) => OnGetSource(source),
                (source) => OnReleaseSource(source),
                (source) => OnDestroySource(source),
                defaultCapacity: capacity
            );
        }

        /// <summary>
        /// オーディオソースを取得
        /// </summary>
        /// <returns></returns>
        public Source GetSorce()
        {
            return _sourcePool.Get();
        }

        /// <summary>
        /// プールを空にする
        /// </summary>
        public void Clear()
        {
            _sourcePool.Clear();
        }

        /// <summary>
        /// プールに入れるインスタンスを新しく生成する際に行う処理
        /// </summary>
        /// <returns></returns>
        private Source OnCreateSource()
        {
            return Object.Instantiate(_source, _parent);
        }

        /// <summary>
        /// プールからインスタンスを取得した際に行う処理
        /// </summary>
        /// <param name="source"></param>
        private void OnGetSource(Source source)
        {
            source.Initialize(() => _sourcePool.Release(source));
        }

        /// <summary>
        /// プールにインスタンスを返却した際に行う処理
        /// </summary>
        /// <param name="source"></param>
        private void OnReleaseSource(Source source)
        {

        }

        /// <summary>
        /// プールから削除される際に行う処理
        /// </summary>
        /// <param name="source"></param>
        private void OnDestroySource(Source source)
        {
            source.Dispose();
            Object.Destroy(source.gameObject);
        }
    }
}
