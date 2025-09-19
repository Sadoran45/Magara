using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    [Header("Detection & Attack")]
    public float detectionRadius = 10f;   // sensing range
    public float attackRange = 2f;        // melee attack range
    public float attackCooldown = 2f;     // delay between attacks
    public float moveSpeed = 3f;          // forward speed

    [Header("Attack Prefab")]
    public GameObject attackPrefab;
    public float spawnDistance = 1.5f;
    public float attackPrefabLifetime = 1f;

    private Transform player;
    private float lastAttackTime = 0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRadius)
        {
            // face the player
            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0; // prevent tilting up/down
            transform.rotation = Quaternion.LookRotation(dir);

            if (distance > attackRange)
            {
                // move forward
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
            }
            else
            {
                // in attack range
                TryAttack();
            }
        }
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Vector3 spawnPos = transform.position + transform.forward * spawnDistance;
            Quaternion spawnRot = transform.rotation;

            GameObject attack = Instantiate(attackPrefab, spawnPos, spawnRot);
            Destroy(attack, attackPrefabLifetime);

            Debug.Log("Monster attacks!");
            lastAttackTime = Time.time;
        }
    }

    // Debug gizmos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
