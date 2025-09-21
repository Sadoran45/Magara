using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts.Gameplay.Characters;

namespace _Game.Scripts.UI
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Canvas healthCanvas;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image healthFillImage;
        [SerializeField] private Image backgroundImage;
        
        [Header("Settings")]
        [SerializeField] private bool hideWhenFull = true;
        [SerializeField] private bool faceCamera = true;
        [SerializeField] private Vector3 offset = Vector3.up * 2f;
        [SerializeField] private float hideDelay = 3f;
        
        [Header("Colors")]
        [SerializeField] private Color healthyColor = Color.green;
        [SerializeField] private Color damagedColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        [SerializeField] private Color backgroundColorNormal = new Color(0, 0, 0, 0.5f);
        [SerializeField] private Color backgroundColorDamaged = new Color(0.2f, 0, 0, 0.7f);
        
        private EnemyCharacter enemyCharacter;
        private Camera mainCamera;
        private float lastDamageTime;
        private bool isVisible = true;

        private void Awake()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
            
            enemyCharacter = GetComponentInParent<EnemyCharacter>();
            if (enemyCharacter == null)
            {
                Debug.LogError($"EnemyHealthBar on {gameObject.name} could not find EnemyCharacter component!");
                return;
            }
            
            InitializeHealthBar();
        }

        private void Start()
        {
            SubscribeToHealthEvents();
            
            // Initially hide if at full health
            if (hideWhenFull && enemyCharacter.HealthPercentage >= 1f)
            {
                SetVisibility(false);
            }
        }

        private void Update()
        {
            // Update health display every frame
            if (enemyCharacter != null)
            {
                UpdateHealthDisplay(enemyCharacter.HealthPercentage);
            }
        }

        private void LateUpdate()
        {
            UpdatePosition();
            UpdateVisibility();
        }

        private void InitializeHealthBar()
        {
            if (healthSlider != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = 1f;
                healthSlider.value = 1f;
            }
            
            if (healthCanvas == null)
            {
                healthCanvas = GetComponentInChildren<Canvas>();
            }
            
            if (healthCanvas != null)
            {
                healthCanvas.worldCamera = mainCamera;
            }
        }

        private void SubscribeToHealthEvents()
        {
            if (enemyCharacter == null) return;

            // Subscribe to damage events
            enemyCharacter.OnDamageReceived += OnDamageReceived;
            
            // Subscribe to death events
            enemyCharacter.OnEnemyDeath += OnEnemyDeath;
        }

        private void UpdateHealthDisplay(float healthPercentage)
        {
            if (healthSlider != null)
            {
                healthSlider.value = healthPercentage;
            }
            
            UpdateHealthColors(healthPercentage);
        }

        private void UpdateHealthColors(float healthPercentage)
        {
            if (healthFillImage == null) return;
            
            Color targetColor;
            Color targetBackgroundColor;
            
            if (healthPercentage > 0.6f)
            {
                targetColor = healthyColor;
                targetBackgroundColor = backgroundColorNormal;
            }
            else if (healthPercentage > 0.3f)
            {
                targetColor = damagedColor;
                targetBackgroundColor = backgroundColorDamaged;
            }
            else
            {
                targetColor = criticalColor;
                targetBackgroundColor = backgroundColorDamaged;
            }
            
            healthFillImage.color = targetColor;
            
            if (backgroundImage != null)
            {
                backgroundImage.color = targetBackgroundColor;
            }
        }

        private void OnDamageReceived(float damage)
        {
            lastDamageTime = Time.time;
            
            // Show health bar when taking damage
            if (!isVisible)
            {
                SetVisibility(true);
            }
        }

        private void OnEnemyDeath()
        {
            // Hide health bar on death
            SetVisibility(false);
        }

        private void UpdatePosition()
        {
            if (faceCamera && mainCamera != null)
            {
                // Face the camera
                Vector3 lookDirection = mainCamera.transform.position - transform.position;
                lookDirection.y = 0; // Keep horizontal
                
                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }
            
            // Update position with offset
            if (enemyCharacter != null)
            {
                transform.position = enemyCharacter.transform.position + offset;
            }
        }

        private void UpdateVisibility()
        {
            if (!hideWhenFull || enemyCharacter == null) return;
            
            bool shouldBeVisible = true;
            
            // Hide if at full health and enough time has passed since last damage
            if (enemyCharacter.HealthPercentage >= 1f && Time.time - lastDamageTime > hideDelay)
            {
                shouldBeVisible = false;
            }
            
            // Hide if enemy is dead
            if (!enemyCharacter.IsAlive)
            {
                shouldBeVisible = false;
            }
            
            if (shouldBeVisible != isVisible)
            {
                SetVisibility(shouldBeVisible);
            }
        }

        private void SetVisibility(bool visible)
        {
            isVisible = visible;
            
            if (healthCanvas != null)
            {
                healthCanvas.gameObject.SetActive(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }

        private void OnDestroy()
        {
            if (enemyCharacter != null)
            {
                enemyCharacter.OnDamageReceived -= OnDamageReceived;
                enemyCharacter.OnEnemyDeath -= OnEnemyDeath;
            }
        }

        // Public methods for external control
        public void ForceShow()
        {
            SetVisibility(true);
            lastDamageTime = Time.time;
        }

        public void ForceHide()
        {
            SetVisibility(false);
        }
    }
}