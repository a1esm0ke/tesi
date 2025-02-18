using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AvatarSetup : MonoBehaviour
{
    public InputField usernameInputField;
    public Button confirmButton;
    public Text feedbackText;  // Opzionale: testo per dare feedback all'utente

    private void Start()
    {
        // Assegna l'evento al bottone di conferma
        confirmButton.onClick.AddListener(OnConfirmName);
    }

    private void OnConfirmName()
    {
        string username = usernameInputField.text.Trim();  // Rimuove eventuali spazi vuoti

        if (!string.IsNullOrEmpty(username))
        {
            // Salva il nome e il flag nei PlayerPrefs
            PlayerPrefs.SetString("PlayerName", username);
            PlayerPrefs.SetInt("PlayerTotalScore", 0);      // Imposta il punteggio iniziale a 0
            PlayerPrefs.SetInt("NicknameSet", 1);           // Flag che indica che il nickname è stato impostato
            PlayerPrefs.Save();

            // Recupera l'istanza del componente AutenticationID
            AutenticationID authID = Object.FindAnyObjectByType<AutenticationID>();
            if (authID != null)
            {
                // Sincronizza i dati iniziali su Firestore (questo creerà il documento se non esiste)
                string userId = PlayerPrefs.GetString("UserId", "");
                authID.SyncUserWithFirestore(userId);
                Debug.Log("Sincronizzazione iniziale completata.");

                // Aggiorna Firestore con i dati iniziali
                string profileImageUrl = PlayerPrefs.GetString("PlayerProfilePhotoPath", "");
                authID.UpdateUserData(username, profileImageUrl);
                Debug.Log("Chiamato UpdateUserData con i seguenti dati: " +
                          "Username: " + username +
                          ", profileImageUrl: " + profileImageUrl +
                          ", totalScore: 0");
            }
            else
            {
                Debug.LogError("Componente AutenticationID non trovato!");
                if (feedbackText != null)
                {
                    feedbackText.text = "Errore: impossibile connettersi al server.";
                }
            }

            // Passa alla scena principale (Main Menu)
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            Debug.LogWarning("Inserisci un nome valido prima di procedere.");
            if (feedbackText != null)
            {
                feedbackText.text = "Inserisci un nome valido!";
            }
        }
    }
}
