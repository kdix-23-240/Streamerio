using Common.Scene;
using UnityEngine;

public class SceneTest : MonoBehaviour
{
    void Start()
    {
        SceneManager.Instance.LoadSceneAsync(SceneType.NewScene);
    }
}
