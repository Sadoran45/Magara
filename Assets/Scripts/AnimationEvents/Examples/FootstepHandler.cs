using UnityEngine;

/// <summary>
/// Example implementation of IAnimationEventListener for handling footstep events.
/// This component demonstrates how to create listeners that respond to specific AnimEventSO events.
/// </summary>
public class FootstepHandler : MonoBehaviour, IAnimationEventListener
{
    [Header("Footstep Events")]
    [Tooltip("Animation events that this handler should listen for")]
    public AnimEventSO[] footstepEvents;
    
    [Header("Audio Settings")]
    [Tooltip("Audio source for playing footstep sounds")]
    public AudioSource audioSource;
    
    [Tooltip("Default footstep sound clips")]
    public AudioClip[] defaultFootstepSounds;
    
    [Range(0f, 1f)]
    [Tooltip("Volume multiplier for footstep sounds")]
    public float volumeMultiplier = 1f;
    
    [Header("Particle Effects")]
    [Tooltip("Particle system for footstep effects")]
    public ParticleSystem footstepParticles;
    
    [Tooltip("Transform representing the left foot position")]
    public Transform leftFootTransform;
    
    [Tooltip("Transform representing the right foot position")]
    public Transform rightFootTransform;

    private AnimationEventDispatcher eventDispatcher;
    private bool isLeftFoot = true; // Alternates between left and right foot

    private void Start()
    {
        // Find the AnimationEventDispatcher on this GameObject or its parent
        eventDispatcher = GetComponentInParent<AnimationEventDispatcher>();
        
        if (eventDispatcher == null)
        {
            Debug.LogError("FootstepHandler: No AnimationEventDispatcher found! Make sure to add one to this GameObject or its parent.", this);
            enabled = false;
            return;
        }

        // Register this handler for all specified footstep events
        RegisterForEvents();

        // Set up audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    private void OnDestroy()
    {
        // Unregister from all events when destroyed
        if (eventDispatcher != null)
        {
            eventDispatcher.UnregisterListenerFromAll(this);
        }
    }

    private void RegisterForEvents()
    {
        if (footstepEvents == null || eventDispatcher == null)
            return;

        foreach (var footstepEvent in footstepEvents)
        {
            if (footstepEvent != null)
            {
                eventDispatcher.RegisterListener(footstepEvent, this);
            }
        }
    }

    /// <summary>
    /// Called when any of the registered animation events are triggered.
    /// </summary>
    /// <param name="animEvent">The triggered animation event</param>
    public void OnAnimationEvent(AnimEventSO animEvent)
    {
        // Check if this is one of our footstep events
        if (IsFootstepEvent(animEvent))
        {
            PlayFootstepSound(animEvent);
            PlayFootstepParticles(animEvent);
            
            // Toggle foot for next step
            isLeftFoot = !isLeftFoot;
            
            Debug.Log($"FootstepHandler: Processed footstep event '{animEvent.eventName}'", this);
        }
    }

    private bool IsFootstepEvent(AnimEventSO animEvent)
    {
        if (footstepEvents == null)
            return false;

        foreach (var footstepEvent in footstepEvents)
        {
            if (footstepEvent == animEvent)
                return true;
        }
        return false;
    }

    private void PlayFootstepSound(AnimEventSO animEvent)
    {
        if (audioSource == null)
            return;

        AudioClip clipToPlay = null;

        // Try to get a custom sound from the event's string parameter (resource path)
        if (!string.IsNullOrEmpty(animEvent.stringParameter))
        {
            clipToPlay = Resources.Load<AudioClip>(animEvent.stringParameter);
        }

        // Fall back to default sounds if no custom sound found
        if (clipToPlay == null && defaultFootstepSounds != null && defaultFootstepSounds.Length > 0)
        {
            clipToPlay = defaultFootstepSounds[Random.Range(0, defaultFootstepSounds.Length)];
        }

        if (clipToPlay != null)
        {
            // Use the float parameter from the event as volume modifier (if provided)
            float volume = volumeMultiplier;
            if (animEvent.floatParameter > 0)
            {
                volume *= animEvent.floatParameter;
            }

            audioSource.PlayOneShot(clipToPlay, volume);
        }
    }

    private void PlayFootstepParticles(AnimEventSO animEvent)
    {
        if (footstepParticles == null)
            return;

        // Determine foot position
        Transform footTransform = isLeftFoot ? leftFootTransform : rightFootTransform;
        
        if (footTransform != null)
        {
            // Move particle system to foot position
            footstepParticles.transform.position = footTransform.position;
        }

        // Use int parameter as burst count (if provided)
        if (animEvent.intParameter > 0)
        {
            var emission = footstepParticles.emission;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0.0f, animEvent.intParameter)
            });
        }

        footstepParticles.Play();
    }

    /// <summary>
    /// Manually trigger a footstep effect (useful for testing)
    /// </summary>
    [ContextMenu("Test Footstep")]
    public void TestFootstep()
    {
        if (footstepEvents != null && footstepEvents.Length > 0 && footstepEvents[0] != null)
        {
            OnAnimationEvent(footstepEvents[0]);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Auto-find components if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        if (footstepParticles == null)
        {
            footstepParticles = GetComponentInChildren<ParticleSystem>();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw gizmos for foot positions
        if (leftFootTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(leftFootTransform.position, 0.1f);
            Gizmos.DrawLine(leftFootTransform.position, leftFootTransform.position + Vector3.up * 0.2f);
        }
        
        if (rightFootTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(rightFootTransform.position, 0.1f);
            Gizmos.DrawLine(rightFootTransform.position, rightFootTransform.position + Vector3.up * 0.2f);
        }
    }
#endif
}