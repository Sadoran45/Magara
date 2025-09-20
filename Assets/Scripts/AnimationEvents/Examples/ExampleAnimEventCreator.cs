using UnityEngine;

/// <summary>
/// Example script for creating common animation event assets.
/// This demonstrates how to create and configure AnimEventSO instances in code.
/// In practice, you would create these as ScriptableObject assets in the editor.
/// </summary>
public class ExampleAnimEventCreator : MonoBehaviour
{
    [Header("Create Example Events")]
    [Tooltip("Click to create example animation event assets")]
    public bool createExampleAssets = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (createExampleAssets)
        {
            createExampleAssets = false;
            CreateExampleEvents();
        }
    }

    [ContextMenu("Create Example Animation Events")]
    private void CreateExampleEvents()
    {
        // Create Assets folder structure
        string folderPath = "Assets/ScriptableObjects/AnimationEvents";
        if (!UnityEditor.AssetDatabase.IsValidFolder(folderPath))
        {
            UnityEditor.AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            UnityEditor.AssetDatabase.CreateFolder("Assets/ScriptableObjects", "AnimationEvents");
        }

        // Footstep events
        CreateAnimEvent("FootstepLeft", "Left foot touches ground", folderPath + "/FootstepLeft.asset", 
            "Audio/Footsteps/Grass", 0.8f, 5, true);
            
        CreateAnimEvent("FootstepRight", "Right foot touches ground", folderPath + "/FootstepRight.asset", 
            "Audio/Footsteps/Grass", 0.8f, 5, true);

        // Combat events
        CreateAnimEvent("AttackHit", "Moment when attack connects with target", folderPath + "/AttackHit.asset", 
            "Audio/Combat/Hit", 1.0f, 10, false);
            
        CreateAnimEvent("AttackStart", "Beginning of attack animation", folderPath + "/AttackStart.asset", 
            "Audio/Combat/Swoosh", 0.6f, 0, false);

        // Jump events
        CreateAnimEvent("JumpStart", "Character begins jumping", folderPath + "/JumpStart.asset", 
            "Audio/Movement/Jump", 0.7f, 3, false);
            
        CreateAnimEvent("JumpLand", "Character lands from jump", folderPath + "/JumpLand.asset", 
            "Audio/Movement/Land", 1.2f, 8, true);

        // UI/Interaction events
        CreateAnimEvent("ButtonPress", "UI button pressed", folderPath + "/ButtonPress.asset", 
            "Audio/UI/Click", 0.5f, 0, false);

        // Special ability events
        CreateAnimEvent("SpellCast", "Magic spell casting moment", folderPath + "/SpellCast.asset", 
            "Audio/Magic/Cast", 1.0f, 15, true);

        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        
        Debug.Log("Created example AnimEventSO assets in " + folderPath);
    }

    private void CreateAnimEvent(string eventName, string description, string assetPath, 
        string audioPath = "", float volumeMultiplier = 1.0f, int particleCount = 0, bool isGroundEvent = false)
    {
        // Check if asset already exists
        if (UnityEditor.AssetDatabase.LoadAssetAtPath<AnimEventSO>(assetPath) != null)
        {
            Debug.Log($"Asset {assetPath} already exists, skipping creation.");
            return;
        }

        var animEvent = ScriptableObject.CreateInstance<AnimEventSO>();
        animEvent.eventName = eventName;
        animEvent.description = description;
        animEvent.stringParameter = audioPath; // Audio file path
        animEvent.floatParameter = volumeMultiplier; // Volume multiplier
        animEvent.intParameter = particleCount; // Particle burst count
        animEvent.boolParameter = isGroundEvent; // Whether this affects ground/environment

        UnityEditor.AssetDatabase.CreateAsset(animEvent, assetPath);
    }
#endif

    /// <summary>
    /// Runtime method to demonstrate event creation (for testing purposes)
    /// </summary>
    public AnimEventSO CreateRuntimeEvent(string name, string desc)
    {
        var animEvent = ScriptableObject.CreateInstance<AnimEventSO>();
        animEvent.eventName = name;
        animEvent.description = desc;
        return animEvent;
    }
}