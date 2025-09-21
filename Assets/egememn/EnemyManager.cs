using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    private static EnemyManager instance;
    public static EnemyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<EnemyManager>();
                if (instance == null)
                {
                    // Create a new GameObject with EnemyManager if it doesn't exist
                    GameObject go = new GameObject("EnemyManager");
                    instance = go.AddComponent<EnemyManager>();
                }
            }
            return instance;
        }
    }
    
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isWaveActive = false;
    
    // Events
    public System.Action OnWaveCompleted;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void StartWave()
    {
        isWaveActive = true;
        activeEnemies.Clear();
        Debug.Log("Wave started - EnemyManager tracking enabled");
    }
    
    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy != null && !activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
            Debug.Log($"Enemy registered: {enemy.name}. Total active enemies: {activeEnemies.Count}");
        }
    }
    
    public void UnregisterEnemy(GameObject enemy)
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
        if (isWaveActive && activeEnemies.Count == 0)
        {
            Debug.Log("All enemies eliminated - Wave completed!");
            isWaveActive = false;
            OnWaveCompleted?.Invoke();
        }
    }
    
    // Called periodically to check for destroyed enemies that weren't properly unregistered
    private void Update()
    {
        if (isWaveActive)
        {
            // Remove null references (destroyed enemies)
            int initialCount = activeEnemies.Count;
            activeEnemies.RemoveAll(e => e == null);
            
            if (activeEnemies.Count != initialCount)
            {
                Debug.Log($"Cleaned up {initialCount - activeEnemies.Count} destroyed enemies. Remaining: {activeEnemies.Count}");
                CheckWaveCompletion();
            }
        }
    }
    
    public int GetActiveEnemyCount()
    {
        // Clean up and return count
        activeEnemies.RemoveAll(e => e == null);
        return activeEnemies.Count;
    }
    
    public bool IsWaveActive()
    {
        return isWaveActive;
    }
    
    public void ForceCompleteWave()
    {
        isWaveActive = false;
        activeEnemies.Clear();
        OnWaveCompleted?.Invoke();
    }
    
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
        isWaveActive = false;
    }
}