using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float playerDetectionRange = 3f;
    
    private GameObject coreTarget;
    private GameObject playerTarget;
    private GameObject currentTarget;
    
    void Start()
    {
        // Core taglı objeyi bul
        coreTarget = GameObject.FindGameObjectWithTag("Core");
        if (coreTarget == null)
        {
            Debug.LogWarning("Core taglı obje bulunamadı!");
        }
        
        // Başlangıçta Core'u hedef al
        currentTarget = coreTarget;
    }

    void Update()
    {
        // Player taglı objeyi kontrol et
        CheckForPlayer();
        
        // Hedefe doğru hareket et
        MoveTowardsTarget();
    }
    
    private void CheckForPlayer()
    {
        // Player taglı objeyi bul
        playerTarget = GameObject.FindGameObjectWithTag("Player");
        
        if (playerTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.transform.position);
            
            // Player 3 birim içindeyse ona yönel
            if (distanceToPlayer <= playerDetectionRange)
            {
                currentTarget = playerTarget;
            }
            // Player 3 birim dışındaysa Core'a geri dön
            else if (currentTarget == playerTarget)
            {
                currentTarget = coreTarget;
            }
        }
        else
        {
            // Player yoksa Core'a git
            currentTarget = coreTarget;
        }
    }
    
    private void MoveTowardsTarget()
    {
        if (currentTarget == null) return;
        
        // Hedefe doğru yön hesapla
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        
        // Sabit hızla hareket et
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Düşmanı hedefe doğru döndür (opsiyonel)
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    // Debug için range'i görselleştir
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRange);
    }
}
