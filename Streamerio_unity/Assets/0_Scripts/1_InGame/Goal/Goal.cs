using UnityEngine;
using InGame;

public class Goal : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            InGameManager.Instance.GameClear();
        }
    }
}