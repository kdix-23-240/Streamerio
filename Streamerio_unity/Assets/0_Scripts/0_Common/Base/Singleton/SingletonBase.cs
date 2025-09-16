using UnityEngine;

namespace Common
{
    /// <summary>
    /// シングルトンパターンのクラスを作るとき用
    /// </summary>
    /// <typeparam name="T">シングルトンパターンにするクラス</typeparam>
    [DisallowMultipleComponent]
    public class SingletonBase<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance is null)
                {
                    // インスタンスを設定
                    _instance = FindObjectOfType<T>();

                    if (_instance is null)
                    {
                        Debug.LogError(typeof(T) + "is nothing");
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            // インスタンスをただ一つにする
            if (this != Instance)
            {
                Destroy(this);
            }
        }
        
        protected virtual void OnDestroy()
        {
            // 破壊されるときに、インスタンスを初期化
            if (this == Instance)
            {
                _instance = null;
            }
        }
    }
}