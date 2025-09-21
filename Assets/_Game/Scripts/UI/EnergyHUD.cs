using _Game.Scripts.Gameplay.Core;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.UI
{
    public class EnergyHUD : MonoBehaviour
    {
        [Header("Energy UI")]
        [SerializeField] private Slider energySlider;
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private Image energyFillImage;
        [SerializeField] private Image energyGlowImage;
        [SerializeField] private CanvasGroup energyUIGroup;
        
        [Header("Energy Colors")]
        [SerializeField] private Color emptyEnergyColor = Color.gray;
        [SerializeField] private Color lowEnergyColor = Color.yellow;
        [SerializeField] private Color mediumEnergyColor = Color.magenta;
        [SerializeField] private Color fullEnergyColor = Color.cyan;
        [SerializeField] private Color glowColor = Color.white;
        
        [Header("Animation Settings")]
        [SerializeField] private float fillAnimationDuration = 0.4f;
        [SerializeField] private float colorTransitionDuration = 0.3f;
        [SerializeField] private float glowPulseDuration = 0.8f;
        [SerializeField] private float glowIntensity = 1.5f;
        [SerializeField] private float gainEffectScale = 1.1f;
        [SerializeField] private float consumeShakeIntensity = 3f;
        
        private EnergySystem energySystem;
        private CompositeDisposable disposables = new();
        
        // DOTween references
        private Tweener energySliderTweener;
        private Tweener colorTweener;
        private Sequence glowSequence;
        private Sequence gainEffectSequence;
        
        // Original transforms
        private Vector3 originalUIScale;
        private Color originalGlowColor;
        
        private void Awake()
        {
            // Store original values
            if (energyUIGroup != null)
                originalUIScale = energyUIGroup.transform.localScale;
            
            if (energyGlowImage != null)
            {
                originalGlowColor = energyGlowImage.color;
                energyGlowImage.color = Color.clear; // Start invisible
            }
        }
        
        public void Initialize(EnergySystem energySystem)
        {
            this.energySystem = energySystem;
            
            if (energySystem != null)
            {
                SetupEnergySubscriptions();
                InitializeUI();
            }
        }
        
        private void InitializeUI()
        {
            // Set initial values
            if (energySlider != null)
            {
                energySlider.maxValue = energySystem.MaxEnergy.Value;
                energySlider.value = energySystem.Energy.Value;
            }
            
            AnimateEnergyText(energySystem.Energy.Value, energySystem.MaxEnergy.Value);
            SetEnergyColor(energySystem.EnergyPercentage.CurrentValue, false);
        }
        
        private void SetupEnergySubscriptions()
        {
            // Subscribe to energy changes
            energySystem.Energy
                .CombineLatest(energySystem.MaxEnergy, (current, max) => new { current, max })
                .Subscribe(energy => AnimateEnergyChange(energy.current, energy.max))
                .AddTo(disposables);
            
            // Subscribe to energy percentage for color changes
            energySystem.EnergyPercentage
                .Subscribe(percentage => AnimateEnergyColor(percentage))
                .AddTo(disposables);
            
            // Subscribe to energy gain events
            energySystem.OnEnergyGained
                .Subscribe(amount => OnEnergyGained(amount))
                .AddTo(disposables);
            
            // Subscribe to energy consume events
            energySystem.OnEnergyConsumed
                .Subscribe(amount => OnEnergyConsumed(amount))
                .AddTo(disposables);
            
            // Subscribe to full energy event for glow effect
            energySystem.OnEnergyFullReached
                .Subscribe(_ => OnEnergyFullReached())
                .AddTo(disposables);
            
            // Stop glow when not full
            energySystem.IsFullEnergy
                .Subscribe(isFull => 
                {
                    if (!isFull)
                    {
                        StopGlowEffect();
                    }
                })
                .AddTo(disposables);
        }
        
        private void AnimateEnergyChange(float newEnergy, float maxEnergy)
        {
            // Update max value immediately
            if (energySlider != null)
            {
                energySlider.maxValue = maxEnergy;
            }
            
            // Animate slider value
            AnimateSliderValue(newEnergy);
            
            // Update text
            AnimateEnergyText(newEnergy, maxEnergy);
        }
        
        private void AnimateSliderValue(float targetValue)
        {
            if (energySlider == null) return;
            
            energySliderTweener?.Kill();
            energySliderTweener = energySlider.DOValue(targetValue, fillAnimationDuration)
                .SetEase(Ease.OutCubic);
        }
        
        private void AnimateEnergyText(float currentEnergy, float maxEnergy)
        {
            if (energyText == null) return;
            
            energyText.DOFade(0.5f, 0.1f)
                .OnComplete(() =>
                {
                    energyText.text = $"{currentEnergy:F0}/{maxEnergy:F0}";
                    energyText.DOFade(1f, 0.1f);
                });
        }
        
        private void AnimateEnergyColor(float energyPercentage)
        {
            SetEnergyColor(energyPercentage, true);
        }
        
        private void SetEnergyColor(float energyPercentage, bool animate)
        {
            if (energyFillImage == null) return;
            
            Color targetColor;
            
            if (energyPercentage <= 0f)
                targetColor = emptyEnergyColor;
            else if (energyPercentage <= 0.33f)
                targetColor = lowEnergyColor;
            else if (energyPercentage <= 0.66f)
                targetColor = mediumEnergyColor;
            else
                targetColor = fullEnergyColor;
            
            if (animate)
            {
                colorTweener?.Kill();
                colorTweener = energyFillImage.DOColor(targetColor, colorTransitionDuration)
                    .SetEase(Ease.OutQuad);
            }
            else
            {
                energyFillImage.color = targetColor;
            }
        }
        
        private void OnEnergyGained(float amount)
        {
            // Scale up effect when gaining energy
            if (energyUIGroup != null)
            {
                gainEffectSequence?.Kill();
                gainEffectSequence = DOTween.Sequence()
                    .Append(energyUIGroup.transform.DOScale(originalUIScale * gainEffectScale, 0.1f))
                    .Append(energyUIGroup.transform.DOScale(originalUIScale, 0.2f))
                    .SetEase(Ease.OutBack);
            }
        }
        
        private void OnEnergyConsumed(float amount)
        {
            // Shake effect when consuming energy
            if (energySlider != null)
            {
                energySlider.transform.DOShakePosition(
                    0.2f, 
                    Vector3.right * consumeShakeIntensity, 
                    vibrato: 8, 
                    randomness: 90f
                );
            }
            
            // Brief flash effect
            if (energyFillImage != null)
            {
                var originalColor = energyFillImage.color;
                energyFillImage.DOColor(Color.white, 0.05f)
                    .OnComplete(() => energyFillImage.DOColor(originalColor, 0.1f));
            }
        }
        
        private void OnEnergyFullReached()
        {
            StartGlowEffect();
        }
        
        private void StartGlowEffect()
        {
            if (energyGlowImage == null) return;
            
            glowSequence?.Kill();
            
            // Pulsing glow effect
            glowSequence = DOTween.Sequence()
                .Append(energyGlowImage.DOColor(glowColor * glowIntensity, glowPulseDuration * 0.5f))
                .Append(energyGlowImage.DOColor(glowColor * 0.3f, glowPulseDuration * 0.5f))
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
            
            // Scale pulsing for extra effect
            energyGlowImage.transform.DOScale(1.2f, glowPulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        
        private void StopGlowEffect()
        {
            glowSequence?.Kill();
            
            if (energyGlowImage != null)
            {
                energyGlowImage.DOColor(Color.clear, 0.3f);
                energyGlowImage.transform.DOScale(1f, 0.3f);
            }
        }
        
        private void OnDestroy()
        {
            // Clean up DOTween animations
            energySliderTweener?.Kill();
            colorTweener?.Kill();
            glowSequence?.Kill();
            gainEffectSequence?.Kill();
            
            if (energySlider != null)
                energySlider.transform.DOKill();
                
            if (energyFillImage != null)
                energyFillImage.DOKill();
                
            if (energyGlowImage != null)
            {
                energyGlowImage.DOKill();
                energyGlowImage.transform.DOKill();
            }
            
            if (energyText != null)
                energyText.DOKill();
                
            if (energyUIGroup != null)
            {
                energyUIGroup.DOKill();
                energyUIGroup.transform.DOKill();
            }
            
            // Clean up R3 subscriptions
            disposables?.Dispose();
            energySystem?.Dispose();
        }
    }
}