using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dispatcher component that receives animation events and forwards them to registered listeners.
/// Attach this component to the GameObject that has the Animator.
/// Animation events should call TriggerEvent method with the AnimEventSO reference.
/// </summary>
public class AnimationEventDispatcher : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("Enable to see debug messages when events are triggered")]
    public bool enableDebugLogs = false;

    // Dictionary to store listeners for each event type
    private Dictionary<AnimEventSO, List<IAnimationEventListener>> eventListeners = 
        new Dictionary<AnimEventSO, List<IAnimationEventListener>>();

    /// <summary>
    /// Method to be called from Animation Events.
    /// Add this as a function call in your animation timeline with an AnimEventSO as Object Reference parameter.
    /// </summary>
    /// <param name="animEvent">The AnimEventSO to trigger</param>
    public void TriggerEvent(AnimEventSO animEvent)
    {
        if (animEvent == null)
        {
            Debug.LogWarning("AnimationEventDispatcher: Received null AnimEventSO!", this);
            return;
        }

        if (enableDebugLogs)
        {
            Debug.Log($"AnimationEventDispatcher: Triggering event '{animEvent.eventName}'", this);
        }

        // Notify all listeners for this specific event
        if (eventListeners.TryGetValue(animEvent, out List<IAnimationEventListener> listeners))
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                var listener = listeners[i];
                if (listener != null)
                {
                    try
                    {
                        listener.OnAnimationEvent(animEvent);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"AnimationEventDispatcher: Error calling listener for event '{animEvent.eventName}': {e.Message}", this);
                    }
                }
                else
                {
                    // Remove null listeners
                    listeners.RemoveAt(i);
                }
            }
        }
        else if (enableDebugLogs)
        {
            Debug.Log($"AnimationEventDispatcher: No listeners registered for event '{animEvent.eventName}'", this);
        }
    }

    /// <summary>
    /// Register a listener for a specific animation event.
    /// </summary>
    /// <param name="animEvent">The event to listen for</param>
    /// <param name="listener">The listener to register</param>
    public void RegisterListener(AnimEventSO animEvent, IAnimationEventListener listener)
    {
        if (animEvent == null || listener == null)
        {
            Debug.LogWarning("AnimationEventDispatcher: Cannot register null event or listener!", this);
            return;
        }

        if (!eventListeners.ContainsKey(animEvent))
        {
            eventListeners[animEvent] = new List<IAnimationEventListener>();
        }

        if (!eventListeners[animEvent].Contains(listener))
        {
            eventListeners[animEvent].Add(listener);
            
            if (enableDebugLogs)
            {
                Debug.Log($"AnimationEventDispatcher: Registered listener for event '{animEvent.eventName}'", this);
            }
        }
    }

    /// <summary>
    /// Unregister a listener from a specific animation event.
    /// </summary>
    /// <param name="animEvent">The event to stop listening for</param>
    /// <param name="listener">The listener to unregister</param>
    public void UnregisterListener(AnimEventSO animEvent, IAnimationEventListener listener)
    {
        if (animEvent == null || listener == null)
            return;

        if (eventListeners.TryGetValue(animEvent, out List<IAnimationEventListener> listeners))
        {
            listeners.Remove(listener);
            
            if (enableDebugLogs)
            {
                Debug.Log($"AnimationEventDispatcher: Unregistered listener for event '{animEvent.eventName}'", this);
            }

            // Clean up empty lists
            if (listeners.Count == 0)
            {
                eventListeners.Remove(animEvent);
            }
        }
    }

    /// <summary>
    /// Unregister a listener from all events.
    /// </summary>
    /// <param name="listener">The listener to completely unregister</param>
    public void UnregisterListenerFromAll(IAnimationEventListener listener)
    {
        if (listener == null)
            return;

        var keysToRemove = new List<AnimEventSO>();

        foreach (var kvp in eventListeners)
        {
            kvp.Value.Remove(listener);
            if (kvp.Value.Count == 0)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        // Clean up empty entries
        foreach (var key in keysToRemove)
        {
            eventListeners.Remove(key);
        }

        if (enableDebugLogs)
        {
            Debug.Log($"AnimationEventDispatcher: Unregistered listener from all events", this);
        }
    }

    /// <summary>
    /// Get the number of listeners for a specific event.
    /// </summary>
    /// <param name="animEvent">The event to check</param>
    /// <returns>Number of listeners</returns>
    public int GetListenerCount(AnimEventSO animEvent)
    {
        if (animEvent == null)
            return 0;

        return eventListeners.TryGetValue(animEvent, out List<IAnimationEventListener> listeners) ? listeners.Count : 0;
    }

    private void OnDestroy()
    {
        // Clear all listeners when the dispatcher is destroyed
        eventListeners.Clear();
    }

#if UNITY_EDITOR
    [Header("Editor Tools")]
    [Space]
    [Tooltip("Test event to trigger in editor for debugging")]
    public AnimEventSO testEvent;

    [ContextMenu("Trigger Test Event")]
    private void TriggerTestEvent()
    {
        if (testEvent != null)
        {
            TriggerEvent(testEvent);
        }
        else
        {
            Debug.LogWarning("No test event assigned!", this);
        }
    }
#endif
}