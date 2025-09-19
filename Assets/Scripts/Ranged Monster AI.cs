using UnityEngine;

public class RangedMonsterAI : MonoBehaviour
{
    [Header("Detection & Movement")]
    public float detectionRadius = 15f;
    public float stopDistance = 8f;       // preferred shooting distance
    public float moveSpeed = 3f;

    [Header("Attack Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float attackCooldown = 2f;
    public float projectileLifetime = 5f;
    public float spawnOffset = 1f;        // how far in front of monster the projectile spawns

    private Transform player;
    private Transform core;
    private float lastAttackTime = 0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        GameObject coreObj = GameObject.FindGameObjectWithTag("Core");
        if (coreObj != null)
            core = coreObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRadius)
        {
            // face the player
            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0; // keep upright
            transform.rotation = Quaternion.LookRotation(dir);

            if (distance > stopDistance)
            {
                // move toward player
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
            }
            else
            {
                // attack if cooldown ready
                TryShoot();
            }
        }
        else if (distance >= detectionRadius)
        {
            Vector3 dir = (core.position - transform.position).normalized;
            dir.y = 0; // prevent tilting up/down
            transform.rotation = Quaternion.LookRotation(dir);

            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
    }

    void TryShoot()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            // spawn projectile in front of monster
            Vector3 spawnPos = transform.position + transform.forward * spawnOffset;
            Quaternion spawnRot = transform.rotation; // just use monster's facing direction

            GameObject proj = Instantiate(projectilePrefab, spawnPos, spawnRot);

            // give it forward velocity (based on monster's facing direction at spawn)
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = transform.forward * projectileSpeed;
            }

            Destroy(proj, projectileLifetime);

            Debug.Log("Ranged monster shoots!");

            lastAttackTime = Time.time;
        }
    }

    // draw gizmos for detection & stop distance
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; // detection radius
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.green; // stop distance
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
