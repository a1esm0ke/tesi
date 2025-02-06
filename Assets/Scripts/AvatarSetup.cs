using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AvatarSetup : MonoBehaviour
{
    public InputField usernameInputField;
    public Button confirmButton;

    private void Start()
    {
        // Assegna l'evento al bottone
        confirmButton.onClick.AddListener(OnConfirmName);
    }

    private void OnConfirmName()
    {
        string username = usernameInputField.text.Trim();  // Rimuove gli spazi vuoti

        if (!string.IsNullOrEmpty(username))
        {
            // Salva il nome nei PlayerPrefs
            PlayerPrefs.SetString("PlayerName", username);
            PlayerPrefs.Save();

            // Vai direttamente al Main Menu
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            Debug.LogWarning("Inserisci un nome valido prima di procedere.");
            // Potresti aggiungere un messaggio visivo per informare l'utente
        }
    }
}
