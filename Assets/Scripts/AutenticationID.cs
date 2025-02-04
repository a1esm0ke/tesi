using UnityEngine;
using Firebase.Auth;
using System.Threading.Tasks;

public class AutenticationID : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    void Start()
    {
        // Inizializza Firebase Authentication
        auth = FirebaseAuth.DefaultInstance;
        AuthenticateAnonymously();
    }

    async void AuthenticateAnonymously()
    {
        try
        {
            // Effettua l'autenticazione anonima
            AuthResult result = await auth.SignInAnonymouslyAsync();

            // Ottieni l'utente autenticato
            currentUser = result.User;

            if (currentUser != null)
            {
                Debug.Log("Utente anonimo autenticato. ID Utente: " + currentUser.UserId);

                // Salva l'ID utente in PlayerPrefs per usarlo in futuro
                PlayerPrefs.SetString("UserId", currentUser.UserId);
                PlayerPrefs.Save();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Errore durante l'autenticazione anonima: " + ex.Message);
        }
    }
}