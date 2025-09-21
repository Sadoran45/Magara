using UnityEngine;
using UnityEngine.UI;
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
    
    [Title("UI Settings")]
    [InfoBox("Assign Legacy UI Text component for wave messages")]
    [SerializeField] private Text waveText;
    
    [Title("Runtime Info")]
    [ReadOnly] [SerializeField] private int currentWaveIndex = 0;
    [ReadOnly] [SerializeField] private int currentEnemyIndex = 0;
    [ReadOnly] [SerializeField] private bool levelCompleted = false;
    [ReadOnly] [SerializeField] private bool waitingForWaveCompletion = false;
    
    private Coroutine spawnCoroutine;
    private List<GameObject> activeEnemies = new List<GameObject>();
    
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
    }
    
    [Button("Reset Level", ButtonSizes.Medium)]
    [GUIColor(1f, 0.8f, 0.4f)]
    public void ResetLevel()
    {
        StopSpawning();
        currentWaveIndex = 0;
        currentEnemyIndex = 0;
        levelCompleted = false;
        waitingForWaveCompletion = false;
        
        // Clear UI
        ClearWaveText();
        
        // Clear all enemies
        ClearAllEnemies();
    }
    
    private bool CanStartSpawning()
    {
        return levelData != null && 
               levelData.waves != null && 
               levelData.waves.Count > 0 && 
               spawnPoints.Count > 0 &&
               !levelCompleted &&
               !waitingForWaveCompletion;
    }
    
    // UI Methods
    private void ShowWaveCountdown(int waveNumber)
    {
        StartCoroutine(WaveCountdownCoroutine(waveNumber));
    }
    
    private IEnumerator WaveCountdownCoroutine(int waveNumber)
    {
        if (waveText == null) yield break;
        
        // Wave başlangıç mesajı
        waveText.text = $"Wave {waveNumber} is Coming";
        yield return new WaitForSeconds(1f);
        
        // Geri sayım
        for (int i = 3; i >= 1; i--)
        {
            waveText.text = $"Wave {waveNumber} is Coming {i}";
            yield return new WaitForSeconds(1f);
        }
        
        // Countdown bitişi
        waveText.text = "";
    }
    
    private void ShowWaveCompleted()
    {
        if (waveText != null)
        {
            StartCoroutine(ShowWaveCompletedCoroutine());
        }
    }
    
    private IEnumerator ShowWaveCompletedCoroutine()
    {
        waveText.text = "Wave Completed";
        yield return new WaitForSeconds(2f);
        waveText.text = "";
    }
    
    private void ShowLevelCompleted()
    {
        if (waveText != null)
        {
            waveText.text = "Level Completed!";
        }
    }
    
    private void ClearWaveText()
    {
        if (waveText != null)
        {
            waveText.text = "";
        }
    }
    
    // Enemy Management Methods
    private void RegisterEnemy(GameObject enemy)
    {
        if (enemy != null && !activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
            Debug.Log($"Enemy registered: {enemy.name}. Total active enemies: {activeEnemies.Count}");
        }
    }
    
    private void UnregisterEnemy(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            Debug.Log($"Enemy unregistered: {enemy.name}. Remaining enemies: {activeEnemies.Count}");
            
            // Clean up null references
            activeEnemies.RemoveAll(e => e == null);
            
            CheckWaveCompletion();
        }
    }
    
    private void CheckWaveCompletion()
    {
        if (waitingForWaveCompletion && activeEnemies.Count == 0)
        {
            Debug.Log("All enemies eliminated - Wave completed!");
            waitingForWaveCompletion = false;
            ShowWaveCompleted();
        }
    }
    
    private void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
        waitingForWaveCompletion = false;
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
        
        if (waveText == null)
        {
            Debug.LogWarning("Wave Text is not assigned! UI messages will not be displayed.");
        }
    }
    
    private IEnumerator SpawnWaves()
    {
        Debug.Log($"SpawnWaves started. Total waves: {levelData.waves.Count}");
        
        while (currentWaveIndex < levelData.waves.Count && !levelCompleted)
        {
            WaveData currentWave = levelData.waves[currentWaveIndex];
            Debug.Log($"Starting Wave {currentWave.waveNumber} with {currentWave.enemySequence.Count} enemies");
            
            // Show wave countdown
            ShowWaveCountdown(currentWave.waveNumber);
            
            // Wait for countdown to finish (4 seconds total: 1 + 3 countdown)
            yield return new WaitForSeconds(4f);
            
            // Clear enemy list and start tracking this wave
            activeEnemies.Clear();
            waitingForWaveCompletion = false;
            
            currentEnemyIndex = 0;
            
            // Spawn all enemies in the wave
            while (currentEnemyIndex < currentWave.enemySequence.Count)
            {
                SpawnSequenceItem currentItem = currentWave.enemySequence[currentEnemyIndex];
                
                if (currentItem.isDelay)
                {
                    // Bu bir delay item'ı
                    Debug.Log($"Processing delay {currentEnemyIndex + 1}/{currentWave.enemySequence.Count}: {currentItem.delayValue} seconds");
                    yield return new WaitForSeconds(currentItem.delayValue);
                }
                else
                {
                    // Bu bir enemy spawn item'ı
                    Debug.Log($"Spawning enemy {currentEnemyIndex + 1}/{currentWave.enemySequence.Count}: {currentItem.enemyType}");
                    SpawnEnemy(currentItem.enemyType);
                    
                    // Normal spawn interval (sadece enemy spawn'dan sonra)
                    if (currentEnemyIndex < currentWave.enemySequence.Count - 1) // Don't wait after last item
                    {
                        Debug.Log($"Waiting {spawnInterval} seconds before next item...");
                        yield return new WaitForSeconds(spawnInterval);
                    }
                }
                
                currentEnemyIndex++;
            }
            
            Debug.Log($"All enemies spawned for Wave {currentWave.waveNumber}. Waiting for wave completion...");
            
            // Wait until all enemies are defeated
            waitingForWaveCompletion = true;
            while (waitingForWaveCompletion)
            {
                // Check for destroyed enemies periodically
                activeEnemies.RemoveAll(e => e == null);
                CheckWaveCompletion();
                
                yield return new WaitForSeconds(0.1f); // Check every 100ms
            }
            
            Debug.Log($"Wave {currentWave.waveNumber} completed!");
            currentWaveIndex++;
            
            // Wait a bit after wave completed message
            yield return new WaitForSeconds(2f);
            
            // Optional: Add delay between waves
            if (currentWaveIndex < levelData.waves.Count)
            {
                Debug.Log("Preparing next wave...");
                yield return new WaitForSeconds(1f);
            }
        }
        
        // All waves completed
        levelCompleted = true;
        Debug.Log("All waves completed! Level finished.");
        
        ShowLevelCompleted();
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
        
        // Register enemy for tracking
        RegisterEnemy(spawnedEnemy);
        
        // Add simple destroy handler for when enemy dies
        StartCoroutine(MonitorEnemyLife(spawnedEnemy));
        
        Debug.Log($"Successfully spawned {enemyType} at {spawnPoint.name} (Position: {spawnPoint.position})");
    }
    
    private IEnumerator MonitorEnemyLife(GameObject enemy)
    {
        while (enemy != null)
        {
            yield return new WaitForSeconds(0.5f); // Check every 500ms
        }
        
        // Enemy was destroyed, unregister it
        UnregisterEnemy(enemy);
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
    
    // Test Methods
    [Button("Kill All Enemies", ButtonSizes.Medium)]
    [GUIColor(0.8f, 0.4f, 0.4f)]
    [EnableIf("@activeEnemies.Count > 0")]
    public void KillAllEnemies()
    {
        Debug.Log($"Killing {activeEnemies.Count} active enemies");
        ClearAllEnemies();
    }
    
    [Button("Force Complete Wave", ButtonSizes.Medium)]
    [GUIColor(0.4f, 0.8f, 0.8f)]
    [EnableIf("waitingForWaveCompletion")]
    public void ForceCompleteWave()
    {
        Debug.Log("Force completing current wave");
        waitingForWaveCompletion = false;
        ShowWaveCompleted();
    }
}
