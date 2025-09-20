using UnityEngine;

public class MockBuddyActions : MonoBehaviour
{

    [SerializeField]
    private GameObject attackPrefab;

    [SerializeField]
    private GameObject defendPrefab;

    public void Attack()
    {
        Instantiate(attackPrefab, transform.position, Quaternion.identity);
    }

    public void Defend()
    {
        Instantiate(defendPrefab, transform.position, Quaternion.identity);
    }
}
