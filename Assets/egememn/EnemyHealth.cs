using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        // Register this enemy with the EnemyManager
        EnemyManager.Instance.RegisterEnemy(gameObject);
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        // Unregister from EnemyManager before destroying
        EnemyManager.Instance.UnregisterEnemy(gameObject);
        
        // You can add death effects, animations, etc. here
        Debug.Log($"{gameObject.name} died!");
        
        // Destroy the enemy
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        // Safety net - make sure enemy is unregistered even if Die() wasn't called
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(gameObject);
        }
    }
    
    // Public methods for external use
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
    
    public void SetHealth(float health)
    {
        maxHealth = health;
        currentHealth = health;
    }
}