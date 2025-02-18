using UnityEngine;

public class FirebaseUpdater : MonoBehaviour
{
    // Intervallo di aggiornamento in secondi (20 secondi in questo caso)
    private float updateInterval = 20.0f;

    private void Start()
    {
        // Rendi questo GameObject persistente in tutte le scene
        DontDestroyOnLoad(gameObject);
        Debug.Log("[FirebaseUpdater] Persistente: questo GameObject non verrà distrutto al cambio scena.");

        // Avvia l'aggiornamento periodico ogni 20 secondi, con un ritardo iniziale di 20 secondi
        InvokeRepeating("UpdateFirebaseData", updateInterval, updateInterval);
        Debug.Log("[FirebaseUpdater] Avviato aggiornamento periodico ogni " + updateInterval + " secondi.");
    }

    private void UpdateFirebaseData()
    {
        // Leggi i dati aggiornati da PlayerPrefs
        string playerName = PlayerPrefs.GetString("PlayerName", "Nuovo Utente");
        string profileImageUrl = PlayerPrefs.GetString("PlayerProfilePhotoPath", "");
        int totalScore = PlayerPrefs.GetInt("PlayerTotalScore", 0);

        // Trova il componente AutenticationID (il tuo Firebase Manager)
        AutenticationID authID = Object.FindAnyObjectByType<AutenticationID>();
        if (authID != null)
        {
            authID.UpdateUserData(playerName, profileImageUrl);
            Debug.Log("[FirebaseUpdater] Aggiornamento periodico: " +
                      "PlayerName = " + playerName +
                      ", profileImageUrl = " + profileImageUrl +
                      ", totalScore = " + totalScore);
        }
        else
        {
            Debug.LogError("[FirebaseUpdater] Firebase Manager (AutenticationID) non trovato.");
        }
    }
}
