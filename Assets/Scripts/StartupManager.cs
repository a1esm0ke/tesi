using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupManager : MonoBehaviour
{
    void Start()
    {
        // Controlla se il nickname è già stato impostato
        if (PlayerPrefs.HasKey("NicknameSet") && PlayerPrefs.GetInt("NicknameSet") == 1)
        {
            // Se il nickname è stato impostato, vai direttamente al Main Menu
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            // Altrimenti mostra la scena di configurazione (AvatarSet)
            SceneManager.LoadScene("AvatarSet");
        }
    }
}
