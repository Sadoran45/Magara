using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    public int damage = 10;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit for " + damage);

            // Example:
            // other.GetComponent<PlayerHealth>().TakeDamage(damage);
        }
    }
}