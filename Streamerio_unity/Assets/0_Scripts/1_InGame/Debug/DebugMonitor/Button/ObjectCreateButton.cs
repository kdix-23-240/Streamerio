using UnityEngine;
using UnityEngine.UI;

namespace DebugTool
{

    public class ObjectCreateButton : MonoBehaviour
    {
        [SerializeField] private GameObject _createdObject; // 生成するオブジェクト

        public void OnClick()
        {
            Instantiate(_createdObject);
        }
    }
}