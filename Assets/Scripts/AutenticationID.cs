using UnityEngine;
using System;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase.Firestore;
using System.Collections.Generic;

public class AutenticationID : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseUser currentUser;
    private FirebaseFirestore db;  // Aggiungi il riferimento a Firestore qui

    void Start()
    {
        // Inizializza Firebase Authentication
        auth = FirebaseAuth.DefaultInstance;
         db = FirebaseFirestore.DefaultInstance;  // Inizializza Firestore
        AuthenticateAnonymously();
    }

async void AuthenticateAnonymously()
{
    try
    {
        AuthResult result = await auth.SignInAnonymouslyAsync();
        currentUser = result.User;
        if (currentUser != null)
        {
            Debug.Log("Utente anonimo autenticato. ID Utente: " + currentUser.UserId);
            PlayerPrefs.SetString("UserId", currentUser.UserId);
            PlayerPrefs.Save();

            // Genera un codice univoco per l'utente
            string uniqueCode = GenerateUniqueCode();
            SyncUserWithFirestore(currentUser.UserId, uniqueCode);
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError("Errore durante l'autenticazione anonima: " + ex.Message);
    }
}

string GenerateUniqueCode()
{
    return Guid.NewGuid().ToString();  // Genera un GUID come codice univoco
}

void SyncUserWithFirestore(string userId, string uniqueCode)
{
    DocumentReference userDocRef = db.Collection("users").Document(userId);
    userDocRef.SetAsync(new { uniqueCode = uniqueCode, competitors = new List<string>() })
        .ContinueWith(task => {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("Sincronizzazione utente con Firestore completata: " + userId);
            }
            else
            {
                Debug.LogError("Errore durante la sincronizzazione con Firestore.");
            }
        });
}

}