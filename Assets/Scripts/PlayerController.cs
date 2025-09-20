using UnityEngine;

public class CustomPlayerController : MonoBehaviour
{
    public enum PlayerType { WASD, ARROWS }
    public enum LocomotionState { Active, Inactive }
    
    public PlayerType playerType;
    public LocomotionState locomotionState = LocomotionState.Active;

    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private Vector3 inputDirection;

    void Update()
    {
        HandleInput();
        MoveAndRotate();
    }

    void HandleInput()
    {
        float h = 0f;
        float v = 0f;

        if (playerType == PlayerType.WASD)
        {
            // WASD
            h = Input.GetKey(KeyCode.A) ? -1 : Input.GetKey(KeyCode.D) ? 1 : 0;
            v = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
            inputDirection = new Vector3(h, 0, v).normalized;
        }
        else if (playerType == PlayerType.ARROWS)
        {
            // Ok tuşları sadece hareket için
            h = Input.GetKey(KeyCode.LeftArrow) ? -1 : Input.GetKey(KeyCode.RightArrow) ? 1 : 0;
            v = Input.GetKey(KeyCode.UpArrow) ? 1 : Input.GetKey(KeyCode.DownArrow) ? -1 : 0;
            inputDirection = new Vector3(h, 0, v).normalized;
        }
    }

    void MoveAndRotate()
    {
        // Locomotion state kontrolü - eğer inactive ise hareket etme
        if (locomotionState == LocomotionState.Inactive)
            return;
            
        if (playerType == PlayerType.WASD)
        {
            // WASD: WASD ile hem hareket hem dönüş
            if (inputDirection.magnitude > 0.1f)
            {
                // Hedef rotasyon
                Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // Hareket
                transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            }
        }
        else if (playerType == PlayerType.ARROWS)
        {
            // ARROWS: Arrow keys ile hareket, mouse ile dönüş
            
            // Mouse pozisyonuna göre dönüş
            Vector3 mousePosition = Input.mousePosition;
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            Vector3 mouseDirection = (mousePosition - screenCenter).normalized;
            
            // Mouse yönünü world space'e çevir
            Vector3 worldDirection = new Vector3(mouseDirection.x, 0, mouseDirection.y);
            
            if (worldDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(worldDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Arrow keys ile hareket
            if (inputDirection.magnitude > 0.1f)
            {
                Vector3 movement = inputDirection * moveSpeed * Time.deltaTime;
                transform.Translate(movement, Space.World);
            }
        }
    }

    // Karakterin baktığı yönü çizelim
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;   // ileri yön çizgisi
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);

        Gizmos.color = Color.green; // sağ yön çizgisi
        Gizmos.DrawLine(transform.position, transform.position + transform.right * 2f);

        Gizmos.color = Color.blue;  // yukarı yön çizgisi
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 2f);
    }
}
