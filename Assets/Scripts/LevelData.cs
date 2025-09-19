using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
#endif
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

public enum EnemyType
{
    Melee,
    Ranged,
    Tank,
    Boss
}

[Serializable]
public class WaveData
{
    [HideInInspector]
    public int waveNumber = 1;
    
    [FoldoutGroup("@GetWaveTitle()", Expanded = true)]
    [InfoBox("Click enemy type buttons below to add them to the spawn sequence")]
    
    [FoldoutGroup("@GetWaveTitle()")]
    [LabelText("Enemy Spawn Sequence")]
    [ListDrawerSettings(ShowFoldout = false, DraggableItems = true, ShowIndexLabels = true)]
    public List<EnemyType> enemySequence = new List<EnemyType>();
    
    [FoldoutGroup("@GetWaveTitle()")]
    [HorizontalGroup("@GetWaveTitle()/EnemyButtons")]
    [Button("Melee")]
    [GUIColor(1f, 0.8f, 0.8f)]
    private void AddMelee() => enemySequence.Add(EnemyType.Melee);
    
    [FoldoutGroup("@GetWaveTitle()")]
    [HorizontalGroup("@GetWaveTitle()/EnemyButtons")]
    [Button("Ranged")]
    [GUIColor(0.8f, 1f, 0.8f)]
    private void AddRanged() => enemySequence.Add(EnemyType.Ranged);
    
    [FoldoutGroup("@GetWaveTitle()")]
    [HorizontalGroup("@GetWaveTitle()/EnemyButtons")]
    [Button("Tank")]
    [GUIColor(0.8f, 0.8f, 1f)]
    private void AddTank() => enemySequence.Add(EnemyType.Tank);
    
    [FoldoutGroup("@GetWaveTitle()")]
    [HorizontalGroup("@GetWaveTitle()/EnemyButtons")]
    [Button("Boss")]
    [GUIColor(1f, 1f, 0.6f)]
    private void AddBoss() => enemySequence.Add(EnemyType.Boss);
    
    [FoldoutGroup("@GetWaveTitle()")]
    [PropertySpace(10)]
    [Button("Clear All")]
    [GUIColor(1f, 0.6f, 0.6f)]
    private void ClearAll() => enemySequence.Clear();
    
    private string GetWaveTitle()
    {
        return $"Wave {waveNumber} - {enemySequence.Count} Enemies";
    }
}

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    [Title("Level Configuration")]
    [InfoBox("Configure waves for this level. Each wave contains a sequence of enemies that will spawn in order.")]
    [ListDrawerSettings(ShowFoldout = false, DraggableItems = true, ShowPaging = false)]
    [LabelText("Waves")]
    [OnValueChanged("UpdateWaveNumbers")]
    public List<WaveData> waves = new List<WaveData>();
    
    [Button("Add New Wave", ButtonSizes.Medium)]
    [GUIColor(0.4f, 0.8f, 1f)]
    private void AddNewWave()
    {
        waves.Add(new WaveData());
        UpdateWaveNumbers();
    }
    

        private void UpdateWaveNumbers()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            waves[i].waveNumber = i + 1;
        }
    }
    
    private void OnValidate()
    {
        UpdateWaveNumbers();
    }
}


