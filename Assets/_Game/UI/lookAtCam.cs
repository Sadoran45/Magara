using UnityEngine;

public class lookAtCam : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera targetCamera;
    
    [Header("Look At Options")]
    public bool invertDirection = false;
    public bool lockYAxis = false;
    public bool smoothRotation = false;
    public float rotationSpeed = 5f;
    
    void Start()
    {
        // Eğer kamera atanmamışsa ana kamerayı kullan
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void Update()
    {
        if (targetCamera == null) return;
        
        // Kameraya bakma işlemini gerçekleştir
        LookAtCamera();
    }
    
    void LookAtCamera()
    {
        Vector3 direction;
        
        if (invertDirection)
        {
            // Kameradan uzaklaşacak şekilde bak
            direction = transform.position - targetCamera.transform.position;
        }
        else
        {
            // Kameraya doğru bak (ters çevrilmiş)
            direction = transform.position - targetCamera.transform.position;
        }
        
        // Y eksenini kilitle (sadece yatay düzlemde dönsün)
        if (lockYAxis)
        {
            direction.y = 0;
        }
        
        // Eğer yön vektörü çok küçükse işlem yapma
        if (direction.sqrMagnitude < 0.001f) return;
        
        // Hedef rotasyonu hesapla ve 180 derece çevir
        Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180, 0);
        
        // Parent'ın rotasyonunu kompanse et (local space'e çevir)
        if (transform.parent != null)
        {
            targetRotation = Quaternion.Inverse(transform.parent.rotation) * targetRotation;
        }
        
        if (smoothRotation)
        {
            // Yumuşak geçiş ile döndür (local rotation)
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Anında döndür (local rotation)
            transform.localRotation = targetRotation;
        }
    }
}
