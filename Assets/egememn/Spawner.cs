using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class Spawner : MonoBehaviour
{
    [Title("Level Configuration")]
    [Required]
    [SerializeField] private LevelData levelData;
    
    [Title("Spawn Points")]
    [InfoBox("Add transform points where enemies can spawn. Enemies will spawn at random points from this list.")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    
    [Title("Enemy Prefabs")]
    [InfoBox("Assign enemy prefabs for each enemy type")]
    [SerializeField] private GameObject meleePrefab;
    [SerializeField] private GameObject rangedPrefab;
    [SerializeField] private GameObject tankPrefab;
    [SerializeField] private GameObject bossPrefab;
    
    [Title("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private bool autoStartSpawning = true;
    
    [Title("Runtime Info")]
    [ReadOnly] [SerializeField] private int currentWaveIndex = 0;
    [ReadOnly] [SerializeField] private int currentEnemyIndex = 0;
    [ReadOnly] [SerializeField] private bool isSpawning = false;
    [ReadOnly] [SerializeField] private bool levelCompleted = false;
    
    private Coroutine spawnCoroutine;
    
    void Start()
    {
        ValidateConfiguration();
        
        if (autoStartSpawning && CanStartSpawning())
        {
            StartSpawning();
        }
    }
    
    [Button("Start Spawning", ButtonSizes.Medium)]
    [EnableIf("@!isSpawning && CanStartSpawning()")]
    [GUIColor(0.4f, 0.8f, 0.4f)]
    public void StartSpawning()
    {
        Debug.Log("StartSpawning called");
        
        if (!CanStartSpawning())
        {
            Debug.LogWarning("Cannot start spawning. Check configuration.");
            Debug.LogWarning($"LevelData: {(levelData != null ? "OK" : "NULL")}");
            Debug.LogWarning($"Waves count: {(levelData?.waves?.Count ?? 0)}");
            Debug.LogWarning($"Spawn points: {spawnPoints.Count}");
            Debug.LogWarning($"Level completed: {levelCompleted}");
            return;
        }
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        
        Debug.Log("Starting spawn coroutine...");
        spawnCoroutine = StartCoroutine(SpawnWaves());
    }
    
    [Button("Stop Spawning", ButtonSizes.Medium)]
    [EnableIf("isSpawning")]
    [GUIColor(0.8f, 0.4f, 0.4f)]
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        isSpawning = false;
    }
    
    [Button("Reset Level", ButtonSizes.Medium)]
    [GUIColor(1f, 0.8f, 0.4f)]
    public void ResetLevel()
    {
        StopSpawning();
        currentWaveIndex = 0;
        currentEnemyIndex = 0;
        levelCompleted = false;
    }
    
    private bool CanStartSpawning()
    {
        return levelData != null && 
               levelData.waves != null && 
               levelData.waves.Count > 0 && 
               spawnPoints.Count > 0 &&
               !levelCompleted;
    }
    
    private void ValidateConfiguration()
    {
        if (levelData == null)
        {
            Debug.LogError("Level Data is not assigned!");
        }
        
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points assigned!");
        }
        
        if (meleePrefab == null || rangedPrefab == null || tankPrefab == null || bossPrefab == null)
        {
            Debug.LogWarning("Some enemy prefabs are not assigned!");
        }
    }
    
    private IEnumerator SpawnWaves()
    {
        isSpawning = true;
        Debug.Log($"SpawnWaves started. Total waves: {levelData.waves.Count}");
        
        while (currentWaveIndex < levelData.waves.Count && !levelCompleted)
        {
            WaveData currentWave = levelData.waves[currentWaveIndex];
            Debug.Log($"Starting Wave {currentWave.waveNumber} with {currentWave.enemySequence.Count} enemies");
            
            currentEnemyIndex = 0;
            
            while (currentEnemyIndex < currentWave.enemySequence.Count)
            {
                EnemyType enemyType = currentWave.enemySequence[currentEnemyIndex];
                Debug.Log($"Spawning enemy {currentEnemyIndex + 1}/{currentWave.enemySequence.Count}: {enemyType}");
                
                SpawnEnemy(enemyType);
                
                currentEnemyIndex++;
                
                // Wait for spawn interval before spawning next enemy
                if (currentEnemyIndex < currentWave.enemySequence.Count) // Don't wait after last enemy
                {
                    Debug.Log($"Waiting {spawnInterval} seconds before next spawn...");
                    yield return new WaitForSeconds(spawnInterval);
                }
            }
            
            Debug.Log($"Wave {currentWave.waveNumber} completed!");
            currentWaveIndex++;
            
            // Optional: Add delay between waves
            if (currentWaveIndex < levelData.waves.Count)
            {
                Debug.Log("Preparing next wave...");
                yield return new WaitForSeconds(spawnInterval * 2f); // 2x interval between waves
            }
        }
        
        // All waves completed
        levelCompleted = true;
        isSpawning = false;
        Debug.Log("All waves completed! Level finished.");
    }
    
    private void SpawnEnemy(EnemyType enemyType)
    {
        Debug.Log($"SpawnEnemy called for: {enemyType}");
        
        GameObject prefabToSpawn = GetEnemyPrefab(enemyType);
        
        if (prefabToSpawn == null)
        {
            Debug.LogError($"No prefab assigned for enemy type: {enemyType}");
            return;
        }
        
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points available!");
            return;
        }
        
        // Select random spawn point
        int randomIndex = Random.Range(0, spawnPoints.Count);
        Transform spawnPoint = spawnPoints[randomIndex];
        
        if (spawnPoint == null)
        {
            Debug.LogError($"Spawn point at index {randomIndex} is null!");
            return;
        }
        
        // Spawn enemy
        GameObject spawnedEnemy = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        
        Debug.Log($"Successfully spawned {enemyType} at {spawnPoint.name} (Position: {spawnPoint.position})");
    }
    
    private GameObject GetEnemyPrefab(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.Melee:
                return meleePrefab;
            case EnemyType.Ranged:
                return rangedPrefab;
            case EnemyType.Tank:
                return tankPrefab;
            case EnemyType.Boss:
                return bossPrefab;
            default:
                return null;
        }
    }
    
    // Public methods for external control
    public void NextWave()
    {
        if (currentWaveIndex < levelData.waves.Count - 1)
        {
            currentWaveIndex++;
            currentEnemyIndex = 0;
        }
    }
    
    public bool IsLevelCompleted()
    {
        return levelCompleted;
    }
    
    public int GetCurrentWaveNumber()
    {
        return currentWaveIndex + 1;
    }
    
    public int GetTotalWaves()
    {
        return levelData != null ? levelData.waves.Count : 0;
    }
}
