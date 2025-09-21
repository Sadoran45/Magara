using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameFromMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Assuming you have a GameManager script that handles game state
        Debug.Log("Game Started!");
        SceneManager.LoadScene("Atmosferik"); // Replace "GameScene" with your actual game scene name
    }
}
