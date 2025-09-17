using UnityEngine;
using UnityEngine.UI;

public class ObjectCreateButton : MonoBehaviour
{
    [SerializeField] private GameObject _createdObject;

    public void OnClick()
    {
        Instantiate(_createdObject);
    }
}