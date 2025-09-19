using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerType { Player1, Player2 }
    public PlayerType playerType;

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

        if (playerType == PlayerType.Player1)
        {
            // WASD
            h = Input.GetKey(KeyCode.A) ? -1 : Input.GetKey(KeyCode.D) ? 1 : 0;
            v = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
        }
        else if (playerType == PlayerType.Player2)
        {
            // Ok tuşları
            h = Input.GetKey(KeyCode.LeftArrow) ? -1 : Input.GetKey(KeyCode.RightArrow) ? 1 : 0;
            v = Input.GetKey(KeyCode.UpArrow) ? 1 : Input.GetKey(KeyCode.DownArrow) ? -1 : 0;
        }

        inputDirection = new Vector3(h, 0, v).normalized;
    }

    void MoveAndRotate()
    {
        if (inputDirection.magnitude > 0.1f)
        {
            // Hedef rotasyon
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Hareket
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
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
