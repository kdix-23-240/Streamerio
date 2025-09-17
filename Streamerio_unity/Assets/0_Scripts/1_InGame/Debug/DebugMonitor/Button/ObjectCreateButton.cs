using UnityEngine;
using UnityEngine.UI;

public class ObjectCreateButton : MonoBehaviour
{
    [SerializeField] private GameObject _createdObject; // 生成するオブジェクト

    public void OnClick()
    {
        Instantiate(_createdObject);
    }
}