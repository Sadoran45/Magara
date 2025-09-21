using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimpleUIManager : MonoBehaviour
{
    [Space(10)]
    [Header("=== UI ELEMENTS ===")]
    [Space(5)]
    
    [SerializeField]
    [Tooltip("Legacy UI Text for wave messages")]
    public Text waveText;
    
    [Space(10)]
    [Header("=== SETTINGS ===")]
    public float countdownDuration = 1f;
    public float messageDisplayDuration = 2f;
    
    void Start()
    {
        if (waveText == null)
        {
            Debug.LogError("Wave Text is not assigned!");
        }
    }
    
    public void StartWaveCountdown(int waveNumber)
    {
        StartCoroutine(ShowWaveCountdown(waveNumber));
    }
    
    private IEnumerator ShowWaveCountdown(int waveNumber)
    {
        if (waveText == null) yield break;
        
        // "Wave X is Coming" mesajı
        waveText.text = $"Wave {waveNumber} is Coming";
        yield return new WaitForSeconds(countdownDuration);
        
        // Countdown 3, 2, 1
        for (int i = 3; i >= 1; i--)
        {
            waveText.text = $"Wave {waveNumber} is Coming {i}";
            yield return new WaitForSeconds(countdownDuration);
        }
        
        // Temizle
        waveText.text = "";
    }
    
    public void ShowWaveCompleted()
    {
        StartCoroutine(ShowWaveCompletedMessage());
    }
    
    private IEnumerator ShowWaveCompletedMessage()
    {
        if (waveText == null) yield break;
        
        waveText.text = "Wave Completed";
        yield return new WaitForSeconds(messageDisplayDuration);
        waveText.text = "";
    }
    
    public void ShowLevelCompleted()
    {
        if (waveText != null)
        {
            waveText.text = "Level Completed!";
        }
    }
    
    public void ClearText()
    {
        if (waveText != null)
        {
            waveText.text = "";
        }
    }
}