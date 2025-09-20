using UnityEngine;

/// <summary>
/// Base ScriptableObject for type-safe animation events.
/// Create instances of this class as assets to represent different animation events.
/// </summary>
[CreateAssetMenu(fileName = "New Animation Event", menuName = "Animation/Animation Event", order = 1)]
public class AnimEventSO : ScriptableObject
{
    [Header("Event Information")]
    [Tooltip("Display name for this animation event")]
    public string eventName;
    
    [Tooltip("Description of what this animation event represents")]
    [TextArea(2, 4)]
    public string description;
    
    [Header("Event Data")]
    [Tooltip("Optional string parameter for the event")]
    public string stringParameter;
    
    [Tooltip("Optional float parameter for the event")]
    public float floatParameter;
    
    [Tooltip("Optional int parameter for the event")]
    public int intParameter;
    
    [Tooltip("Optional bool parameter for the event")]
    public bool boolParameter;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(eventName))
        {
            eventName = name;
        }
    }

    public override string ToString()
    {
        return eventName;
    }
}