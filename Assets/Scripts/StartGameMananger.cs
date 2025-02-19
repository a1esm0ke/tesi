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
            startButton.onClick.AddListener(LoadMiniGameScene);
        }
    }

    void LoadMiniGameScene()
    {
        SceneManager.LoadScene("MiniGameScene"); // Assicurati che il nome sia corretto
    }
}
