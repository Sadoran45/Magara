using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System.Reflection;

#if UNITY_EDITOR
[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : OdinEditor
{
    private static bool allWavesCollapsed = false;
    
    public override void OnInspectorGUI()
    {
        // Handle keyboard events
        HandleKeyboardInput();
        
        // Draw the default inspector
        base.OnInspectorGUI();
    }
    
    private void HandleKeyboardInput()
    {
        Event currentEvent = Event.current;
        
        // Check if a key is pressed
        if (currentEvent.type == EventType.KeyDown)
        {
            // Press 'C' key to collapse/expand all waves
            if (currentEvent.keyCode == KeyCode.C && !currentEvent.control && !currentEvent.alt && !currentEvent.shift)
            {
                ToggleAllWaveFoldouts();
                currentEvent.Use(); // Consume the event
            }
            // Press 'M' key to minimize all waves (collapse them)
            else if (currentEvent.keyCode == KeyCode.M && !currentEvent.control && !currentEvent.alt && !currentEvent.shift)
            {
                MinimizeAllWaves();
                currentEvent.Use(); // Consume the event
            }
            // Press 'E' key to expand all waves
            else if (currentEvent.keyCode == KeyCode.E && !currentEvent.control && !currentEvent.alt && !currentEvent.shift)
            {
                ExpandAllWaves();
                currentEvent.Use(); // Consume the event
            }
        }
    }
    
    private void ToggleAllWaveFoldouts()
    {
        allWavesCollapsed = !allWavesCollapsed;
        
        if (allWavesCollapsed)
        {
            MinimizeAllWaves();
        }
        else
        {
            ExpandAllWaves();
        }
    }
    
    private void MinimizeAllWaves()
    {
        allWavesCollapsed = true;
        SetAllWavesFoldoutState(false);
    }
    
    private void ExpandAllWaves()
    {
        allWavesCollapsed = false;
        SetAllWavesFoldoutState(true);
    }
    
    private void SetAllWavesFoldoutState(bool expanded)
    {
        LevelData levelData = (LevelData)target;
        
        // Use Unity's EditorPrefs to store foldout states
        for (int i = 0; i < levelData.waves.Count; i++)
        {
            string foldoutKey = GetFoldoutKey(levelData, i);
            EditorPrefs.SetBool(foldoutKey, expanded);
        }
        
        // Force repaint to update the inspector
        Repaint();
        EditorUtility.SetDirty(target);
        
        // Trigger a GUI update
        GUIUtility.ExitGUI();
    }
    
    private string GetFoldoutKey(LevelData levelData, int waveIndex)
    {
        // Create a unique key for each wave's foldout state based on the asset instance ID and wave index
        return $"LevelData_{levelData.GetInstanceID()}_Wave_{waveIndex}_Foldout";
    }
}
#endif