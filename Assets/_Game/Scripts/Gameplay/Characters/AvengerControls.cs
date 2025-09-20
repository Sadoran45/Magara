using UnityEngine;

namespace _Game.Scripts.Gameplay.Characters
{
    public class AvengerControls : MonoBehaviour
    {
        
        [SerializeField] private AvengerCharacter playerMotor;
        
        private void Update()
        {
            HandleMovement();

            HandleJumpSlam();
            
            HandleAutoAttack();
            
        }

        private void HandleJumpSlam()
        {
            
        }

        private void HandleAutoAttack()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("On mouse click");
                playerMotor.AutoAttack().Forget();
            }
            
        }
        
        private void HandleMovement()
        {
            // Get movement input from arrow keys (not WASD)
            float inputX = 0f, inputY = 0f;
            if (Input.GetKey(KeyCode.A)) inputX = -1f;
            if (Input.GetKey(KeyCode.D)) inputX = 1f;
            if (Input.GetKey(KeyCode.W)) inputY = 1f;
            if (Input.GetKey(KeyCode.S)) inputY = -1f;
            
            var inputVector = new Vector3(inputX, 0, inputY).normalized;
            playerMotor.SendMovementInput(inputVector);
            playerMotor.SetAimDirection(inputVector);
        }
    }
}