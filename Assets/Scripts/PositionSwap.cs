using UnityEngine;

public class PositionSwap : MonoBehaviour
{
    [Header("Objects to Swap")]
    [SerializeField] private Transform P1; // P1 objesi
    [SerializeField] private Transform P2; // P2 objesi
    
    [Header("Particle Effects")]
    [SerializeField] private GameObject particleEffectPrefab; // Particle effect prefab'ı
    [SerializeField] private float particlePlayDuration = 2f; // Particle'ların ne kadar süre oynayacağı

    void Update()
    {
        // P tuşuna basıldığında pozisyonları değiştir
        if (Input.GetKeyDown(KeyCode.P))
        {
            SwapPositions();
        }
    }
    
    /// <summary>
    /// P1 ve P2 objelerinin pozisyonlarını birbirleriyle değiştirir
    /// </summary>
    private void SwapPositions()
    {
        // Her iki obje de var mı kontrol et
        if (P1 == null || P2 == null)
        {
            Debug.LogWarning("P1 veya P2 objesi atanmamış!");
            return;
        }
        
        // Pozisyonları geçici olarak sakla
        Vector3 tempPosition = P1.position;
        
        // P1'i P2'nin pozisyonuna taşı
        P1.position = P2.position;
        
        // P2'yi P1'in eski pozisyonuna taşı
        P2.position = tempPosition;
        
        // Particle effect'leri oynat
        PlayParticleEffects();
        
        Debug.Log($"Pozisyonlar değiştirildi! P1: {P1.position}, P2: {P2.position}");
    }
    
    /// <summary>
    /// Her iki pozisyonda da particle effect'leri oynatır
    /// </summary>
    private void PlayParticleEffects()
    {
        // P1 pozisyonunda particle effect instantiate et
        if (particleEffectPrefab != null && P1 != null)
        {
            GameObject p1Effect = Instantiate(particleEffectPrefab, P1.position, P1.rotation);
            Destroy(p1Effect, particlePlayDuration);
        }
        else
        {
            Debug.LogWarning("Particle Effect Prefab veya P1 objesi atanmamış!");
        }
        
        // P2 pozisyonunda particle effect instantiate et
        if (particleEffectPrefab != null && P2 != null)
        {
            GameObject p2Effect = Instantiate(particleEffectPrefab, P2.position, P2.rotation);
            Destroy(p2Effect, particlePlayDuration);
        }
        else
        {
            Debug.LogWarning("Particle Effect Prefab veya P2 objesi atanmamış!");
        }
        
        Debug.Log($"Particle effect'ler {particlePlayDuration} saniye boyunca oynatılacak");
    }
    
    /// <summary>
    /// P1 ve P2 objelerini manuel olarak atamak için kullanılabilir
    /// </summary>
    public void SetObjects(Transform newP1, Transform newP2)
    {
        P1 = newP1;
        P2 = newP2;
        Debug.Log("P1 ve P2 objeleri atandı");
    }
}
