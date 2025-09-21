using UnityEngine;

public class hitbymissile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Called when a trigger collider enters this object's trigger collider
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Missile"))
        {
            Debug.Log($"Hit by missile: {other.name}");
        }
    }
}
