using UnityEngine;

public class MagicMissile : MonoBehaviour
{
    public int damage = 10;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit by magic missile for " + damage + " damage!");

            // Example:
            // other.GetComponent<PlayerHealth>().TakeDamage(damage);

            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            // Destroy if hits wall or environment
            Destroy(gameObject);
        }
    }
}
