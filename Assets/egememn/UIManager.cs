using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Legacy UI Text component for displaying wave messages")]
    public Text waveText;
    
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<UIManager>();
                if (instance == null)
                {
                    Debug.LogError("UIManager not found in scene!");
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void ShowWaveCountdown(int waveNumber)
    {
        StartCoroutine(WaveCountdownCoroutine(waveNumber));
    }
    
    private IEnumerator WaveCountdownCoroutine(int waveNumber)
    {
        if (waveText == null)
        {
            Debug.LogWarning("Wave Text is not assigned in UIManager!");
            yield break;
        }
        
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
    
    public void ShowWaveCompleted()
    {
        if (waveText == null)
        {
            Debug.LogWarning("Wave Text is not assigned in UIManager!");
            return;
        }
        
        StartCoroutine(ShowWaveCompletedCoroutine());
    }
    
    private IEnumerator ShowWaveCompletedCoroutine()
    {
        waveText.text = "Wave Completed";
        yield return new WaitForSeconds(2f);
        waveText.text = "";
    }
    
    public void ShowLevelCompleted()
    {
        if (waveText == null)
        {
            Debug.LogWarning("Wave Text is not assigned in UIManager!");
            return;
        }
        
        waveText.text = "Level Completed!";
    }
    
    public void ClearText()
    {
        if (waveText != null)
        {
            waveText.text = "";
        }
    }
}