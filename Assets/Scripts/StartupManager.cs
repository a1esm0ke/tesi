using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupManager : MonoBehaviour
{
    void Start()
    {
        // Controlla se il nickname è già stato impostato
        if (PlayerPrefs.HasKey("NicknameSet") && PlayerPrefs.GetInt("NicknameSet") == 1)
        {
            Debug.Log("Nickname già impostato. Carico il MainMenu.");
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            Debug.Log("Nickname non impostato. Carico AvatarSet per la configurazione.");
            SceneManager.LoadScene("AvatarSet");
        }
    }
}
