using _Game.Scripts.Gameplay.Characters;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using R3;

namespace _Game.Scripts.UI
{
    public class SurvivorHUD : MonoBehaviour
    {
        [Header("Health UI")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Image healthFillImage;
        [SerializeField] private CanvasGroup healthUIGroup;
        
        [Header("Health Colors")]
        [SerializeField] private Color fullHealthColor = Color.green;
        [SerializeField] private Color mediumHealthColor = Color.yellow;
        [SerializeField] private Color lowHealthColor = Color.red;
        [SerializeField] private Color criticalHealthColor = new Color(0.8f, 0f, 0f);
        
        [Header("Animation Settings")]
        [SerializeField] private float sliderAnimationDuration = 0.3f;
        [SerializeField] private float colorTransitionDuration = 0.2f;
        [SerializeField] private float pulseIntensity = 0.3f;
        [SerializeField] private float pulseDuration = 0.6f;
        [SerializeField] private float shakeIntensity = 5f;
        [SerializeField] private float shakeDuration = 0.2f;
        
        [Header("Low Health Warning")]
        [SerializeField] private float lowHealthThreshold = 0.3f;
        [SerializeField] private float criticalHealthThreshold = 0.15f;
        
        private SurvivorCharacter survivorCharacter;
        private CompositeDisposable disposables = new();
        
        // DOTween sequences and tweeners
        private Sequence pulseSequence;
        private Tweener sliderTweener;
        private Tweener colorTweener;
        private bool isLowHealthWarning;
        private bool isCriticalHealthWarning;
        
        // Original positions and scales for shake/pulse effects
        private Vector3 originalSliderPosition;
        private Vector3 originalHealthUIScale;
        
        private void Awake()
        {
            // Store original transforms
            if (healthSlider != null)
                originalSliderPosition = healthSlider.transform.localPosition;
            
            if (healthUIGroup != null)
                originalHealthUIScale = healthUIGroup.transform.localScale;
        }
        
        public void Initialize(SurvivorCharacter survivorCharacter)
        {
            this.survivorCharacter = survivorCharacter;
            
            if (survivorCharacter?.HealthSystem != null)
            {
                SetupHealthSubscriptions();
                InitializeUI();
            }
        }
        
        private void InitializeUI()
        {
            var healthSystem = survivorCharacter.HealthSystem;
            
            // Set initial values without animation
            if (healthSlider != null)
            {
                healthSlider.maxValue = healthSystem.MaxHealth.Value;
                healthSlider.value = healthSystem.Health.Value;
            }
            
            
            SetHealthColor(healthSystem.HealthPercentage.CurrentValue, false);
        }
        
        private void SetupHealthSubscriptions()
        {
            var healthSystem = survivorCharacter.HealthSystem;
            
            // Subscribe to health changes with smooth animation
            healthSystem.Health
                .CombineLatest(healthSystem.MaxHealth, (current, max) => new { current, max })
                .Subscribe(health => AnimateHealthChange(health.current, health.max))
                .AddTo(disposables);
            
            // Subscribe to health percentage for color and warning effects
            healthSystem.HealthPercentage
                .Subscribe(percentage => 
                {
                    AnimateHealthColor(percentage);
                    HandleHealthWarnings(percentage);
                })
                .AddTo(disposables);
            
            // Subscribe to damage events for shake effect
            healthSystem.OnDamageDealt
                .Subscribe(damage => OnDamageReceived(damage))
                .AddTo(disposables);
            
            // Subscribe to death
            healthSystem.IsAlive
                .Subscribe(isAlive => 
                {
                    if (!isAlive)
                    {
                        AnimateDeathUI();
                    }
                })
                .AddTo(disposables);
        }
        
        private void AnimateHealthChange(float newHealth, float maxHealth)
        {
            // Update max value immediately
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
            }
            
            // Animate slider value change
            AnimateSliderValue(newHealth);
            
            // Update text with typewriter effect
            AnimateHealthText(newHealth, maxHealth);
        }
        
        private void AnimateSliderValue(float targetValue)
        {
            if (healthSlider == null) return;
            
            // Kill previous tween
            sliderTweener?.Kill();
            
            // Animate slider with easing
            sliderTweener = healthSlider.DOValue(targetValue, sliderAnimationDuration)
                .SetEase(Ease.OutQuart);
        }
        
        private void AnimateHealthText(float currentHealth, float maxHealth)
        {
            if (healthText == null || !survivorCharacter.HealthSystem.IsAlive.CurrentValue) return;
            
            // Simple text update with fade effect
            healthText.DOFade(0f, 0.1f)
                .OnComplete(() =>
                {
                    healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
                    healthText.DOFade(1f, 0.1f);
                });
        }
        
        private void AnimateHealthColor(float healthPercentage)
        {
            SetHealthColor(healthPercentage, true);
        }
        
        private void SetHealthColor(float healthPercentage, bool animate)
        {
            if (healthFillImage == null) return;
            
            Color targetColor;
            
            if (healthPercentage <= criticalHealthThreshold)
                targetColor = criticalHealthColor;
            else if (healthPercentage <= lowHealthThreshold)
                targetColor = lowHealthColor;
            else if (healthPercentage <= 0.6f)
                targetColor = mediumHealthColor;
            else
                targetColor = fullHealthColor;
            
            if (animate)
            {
                // Kill previous color tween
                colorTweener?.Kill();
                
                // Animate color transition
                colorTweener = healthFillImage.DOColor(targetColor, colorTransitionDuration)
                    .SetEase(Ease.OutQuad);
            }
            else
            {
                healthFillImage.color = targetColor;
            }
        }
        
        private void HandleHealthWarnings(float healthPercentage)
        {
            bool shouldShowCritical = healthPercentage <= criticalHealthThreshold;
            bool shouldShowLow = healthPercentage <= lowHealthThreshold && !shouldShowCritical;
            
            // Critical health warning
            if (shouldShowCritical && !isCriticalHealthWarning)
            {
                StartCriticalHealthWarning();
            }
            else if (!shouldShowCritical && isCriticalHealthWarning)
            {
                StopCriticalHealthWarning();
            }
            
            // Low health warning
            if (shouldShowLow && !isLowHealthWarning)
            {
                StartLowHealthWarning();
            }
            else if (!shouldShowLow && isLowHealthWarning)
            {
                StopLowHealthWarning();
            }
        }
        
        private void StartLowHealthWarning()
        {
            isLowHealthWarning = true;
            
            // Gentle pulse effect for low health
            if (healthUIGroup != null)
            {
                pulseSequence?.Kill();
                pulseSequence = DOTween.Sequence()
                    .Append(healthUIGroup.DOFade(1f - pulseIntensity * 0.3f, pulseDuration * 0.8f))
                    .Append(healthUIGroup.DOFade(1f, pulseDuration * 0.8f))
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
        
        private void StartCriticalHealthWarning()
        {
            isCriticalHealthWarning = true;
            StopLowHealthWarning(); // Stop low health warning
            
            // Intense pulse effect for critical health
            if (healthUIGroup != null)
            {
                pulseSequence?.Kill();
                pulseSequence = DOTween.Sequence()
                    .Append(healthUIGroup.transform.DOScale(originalHealthUIScale * (1f + pulseIntensity), pulseDuration * 0.4f))
                    .Append(healthUIGroup.transform.DOScale(originalHealthUIScale, pulseDuration * 0.4f))
                    .Join(healthUIGroup.DOFade(0.7f, pulseDuration * 0.4f))
                    .Append(healthUIGroup.DOFade(1f, pulseDuration * 0.4f))
                    .SetLoops(-1, LoopType.Restart)
                    .SetEase(Ease.InOutQuad);
            }
        }
        
        private void StopLowHealthWarning()
        {
            isLowHealthWarning = false;
            
            pulseSequence?.Kill();
            
            if (healthUIGroup != null)
            {
                healthUIGroup.DOFade(1f, 0.2f);
            }
        }
        
        private void StopCriticalHealthWarning()
        {
            isCriticalHealthWarning = false;
            
            pulseSequence?.Kill();
            
            if (healthUIGroup != null)
            {
                healthUIGroup.transform.DOScale(originalHealthUIScale, 0.2f);
                healthUIGroup.DOFade(1f, 0.2f);
            }
        }
        
        private void OnDamageReceived(float damage)
        {
            // Shake effect when taking damage
            if (healthSlider != null)
            {
                healthSlider.transform.DOShakePosition(
                    shakeDuration, 
                    Vector3.right * shakeIntensity, 
                    vibrato: 10, 
                    randomness: 90f
                ).OnComplete(() => 
                {
                    // Reset to original position
                    healthSlider.transform.localPosition = originalSliderPosition;
                });
            }
            
            // Flash effect on damage
            if (healthFillImage != null)
            {
                var originalColor = healthFillImage.color;
                healthFillImage.DOColor(Color.white, 0.05f)
                    .OnComplete(() => 
                    {
                        healthFillImage.DOColor(originalColor, 0.15f);
                    });
            }
        }
        
        private void AnimateDeathUI()
        {
            // Stop all warning animations
            StopLowHealthWarning();
            StopCriticalHealthWarning();
            
            // Death animation sequence
            if (healthText != null)
            {
                healthText.DOColor(Color.red, 0.3f);
                healthText.transform.DOShakeScale(0.5f, 0.3f)
                    .OnComplete(() =>
                    {
                        healthText.text = "DEAD";
                        healthText.transform.DOScale(1.2f, 0.2f)
                            .SetEase(Ease.OutBack);
                    });
            }
            
            if (healthUIGroup != null)
            {
                healthUIGroup.DOFade(0.6f, 0.5f);
            }
        }
        
        private void OnDestroy()
        {
            // Clean up DOTween animations
            sliderTweener?.Kill();
            colorTweener?.Kill();
            pulseSequence?.Kill();
            
            // Clean up health UI tweens
            if (healthSlider != null)
                healthSlider.transform.DOKill();
            
            if (healthFillImage != null)
                healthFillImage.DOKill();
            
            if (healthText != null)
            {
                healthText.DOKill();
                healthText.transform.DOKill();
            }
            
            if (healthUIGroup != null)
            {
                healthUIGroup.DOKill();
                healthUIGroup.transform.DOKill();
            }
            
            // Clean up R3 subscriptions
            disposables?.Dispose();
        }
    }
}