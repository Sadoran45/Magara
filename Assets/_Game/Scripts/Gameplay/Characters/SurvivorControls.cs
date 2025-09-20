using UnityEngine;

namespace _Game.Scripts.Gameplay.Characters
{
    public class SurvivorControls : MonoBehaviour
    {
        [SerializeField] private SurvivorCharacter playerMotor;
        [SerializeField] private LayerMask planeLayerMask;
        
        private void Update()
        {
            HandleMovement();

            HandleAutoAttack();
        }

        private void HandleAutoAttack()
        {
            // On left mouse click
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("On mouse click");
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(mouseRay, out hit, Mathf.Infinity, planeLayerMask))
                {
                    Debug.Log("Hit position: "  + hit.point);
                    
                    var simplifiedHitPoint = new Vector3(hit.point.x, 0, hit.point.z);
                    var simplifiedPosition = new Vector3(transform.position.x, 0, transform.position.z);
                    
                    var direction =  simplifiedHitPoint - simplifiedPosition;

                    playerMotor.AutoAttack(direction).Forget();
                }
                
            }
        }
        
        private void HandleMovement()
        {
            // Get movement input from arrow keys (not WASD)
            float inputX = 0f, inputY = 0f;
            if (Input.GetKey(KeyCode.LeftArrow)) inputX = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) inputX = 1f;
            if (Input.GetKey(KeyCode.UpArrow)) inputY = 1f;
            if (Input.GetKey(KeyCode.DownArrow)) inputY = -1f;
            
            var inputVector = new Vector3(inputX, 0, inputY).normalized;
            playerMotor.SendMovementInput(inputVector);
        }
    }
}