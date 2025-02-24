using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    public Button startButton; // Il bottone invisibile

    void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(LoadRandomMiniGameScene);
        }
    }

    void LoadRandomMiniGameScene()
    {
        // Scegli una scena a caso: 50% MiniGameScene, 50% MiniGame2Scene
        if (Random.value < 0.1f)
        {
            SceneManager.LoadScene("MiniGameScene");
        }
        else
        {
            SceneManager.LoadScene("MiniGame2Scene");
        }
    }
}
