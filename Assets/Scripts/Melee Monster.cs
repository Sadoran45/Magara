using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    [Header("Detection & Attack")]
    public float detectionRadius = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    public GameObject attackHitbox; // assign child collider object
    public float hitboxActiveTime = 0.5f; // how long hitbox stays active

    private NavMeshAgent agent;
    private Transform player;
    private float lastAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        /* optional: create detection trigger at runtime
        SphereCollider col = gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = detectionRadius;*/

        // make sure hitbox starts disabled
        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
            agent.ResetPath();
        }
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > attackRange)
            {
                agent.SetDestination(player.position);
            }
            else
            {
                agent.ResetPath();
                TryAttack();
            }
        }
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Debug.Log("Monster starts attack!");
            StartCoroutine(ActivateHitbox());
            lastAttackTime = Time.time;
        }
    }

    System.Collections.IEnumerator ActivateHitbox()
    {
        attackHitbox.SetActive(true);
        yield return new WaitForSeconds(hitboxActiveTime);
        attackHitbox.SetActive(false);
    }
}
