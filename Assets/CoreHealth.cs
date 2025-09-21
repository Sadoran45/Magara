using UnityEngine;
using UnityEngine.UI;

public class CoreHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    
    [Header("UI")]
    public Slider healthSlider;
    
    [Header("Damage Settings")]
    public float mob1DamagePerSecond = 3f;
    public float mob2DamagePerSecond = 10f;
    
    private bool isCollidingWithMob1 = false;
    private bool isCollidingWithMob2 = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }
    
    void Update()
    {
        // Can azaltma işlemi
        if (isCollidingWithMob1)
        {
            TakeDamage(mob1DamagePerSecond * Time.deltaTime);
        }
        
        if (isCollidingWithMob2)
        {
            TakeDamage(mob2DamagePerSecond * Time.deltaTime);
        }
    }
    
    private void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthUI();
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        Debug.Log("Core destroyed!");
        // Burada ölüm işlemlerini yapabilirsiniz
        // exit the game
        Application.Quit();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("mob1"))
        {
            isCollidingWithMob1 = true;
        }
        else if (other.CompareTag("mob2"))
        {
            isCollidingWithMob2 = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("mob1"))
        {
            isCollidingWithMob1 = false;
        }
        else if (other.CompareTag("mob2"))
        {
            isCollidingWithMob2 = false;
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("mob1"))
        {
            isCollidingWithMob1 = true;
        }
        else if (collision.gameObject.CompareTag("mob2"))
        {
            isCollidingWithMob2 = true;
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("mob1"))
        {
            isCollidingWithMob1 = false;
        }
        else if (collision.gameObject.CompareTag("mob2"))
        {
            isCollidingWithMob2 = false;
        }
    }
}
