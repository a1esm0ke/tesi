using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using System.Collections.Generic;
using System;
using Firebase.Extensions;

public class AutenticationID : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseUser currentUser;
    private FirebaseFirestore db;

    void Start()
    {
        DontDestroyOnLoad(gameObject);  // Mantiene l'oggetto attivo tra le scene
        Debug.Log("[AutenticationID] In Start: initializing Firebase Authentication and Firestore.");
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        AuthenticateAnonymously();
    }

private async void AuthenticateAnonymously()
{
    Debug.Log("[AutenticationID] Attempting anonymous authentication...");
    
    try
    {
        AuthResult result = await auth.SignInAnonymouslyAsync();
        currentUser = result.User;

        if (currentUser == null)
        {
            Debug.LogError("[AutenticationID] Authentication succeeded but currentUser is NULL! Ritento il recupero dell'utente...");
            currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        }

        if (currentUser != null)
        {
            string userId = currentUser.UserId;
            Debug.Log("[AutenticationID] Anonymous user authenticated. UserID: " + userId);

            // Salva UserId in PlayerPrefs
            PlayerPrefs.SetString("UserId", userId);
            PlayerPrefs.Save();

            // Sincronizza i dati utente con Firestore
            SyncUserWithFirestore(userId);
        }
        else
        {
            Debug.LogError("[AutenticationID] ERRORE: Impossibile recuperare l'utente dopo il login anonimo!");
        }
    }
    catch (Exception ex)
    {
        Debug.LogError("[AutenticationID] Error during anonymous authentication: " + ex.Message);
    }
}


public void SyncUserWithFirestore(string userId)
{
    if (string.IsNullOrEmpty(userId))
    {
        Debug.LogError("[AutenticationID] ❌ ERRORE: userId è VUOTO! Firestore non può sincronizzare un ID vuoto.");
        return;
    }

    Debug.Log("[AutenticationID] 🔄 SyncUserWithFirestore chiamato per userId: " + userId);
    DocumentReference userDocRef = db.Collection("users").Document(userId);

    userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted && task.Result.Exists)
        {
            Debug.Log("[AutenticationID] ✅ L'utente esiste già su Firestore. Nessuna azione necessaria.");
        }
        else
        {
            Debug.Log("[AutenticationID] 🆕 Creazione di un nuovo documento utente su Firestore...");
            Dictionary<string, object> userData = new Dictionary<string, object>
            {
                { "name", "Nuovo Utente" },
                { "profileImageUrl", "" },
                { "totalScore", 0 },
                { "check-mark1", false },
                { "check-mark2", false },
                { "check-mark3", false },
                { "esercizio1", "" },
                { "esercizio2", "" },
                { "esercizio3", "" }
            };

            userDocRef.SetAsync(userData).ContinueWithOnMainThread(saveTask =>
            {
                if (saveTask.IsCompleted)
                    Debug.Log("[AutenticationID] ✅ Nuovo utente creato su Firestore con successo!");
                else
                    Debug.LogError("[AutenticationID] ❌ Errore nella creazione del documento utente: " + saveTask.Exception);
            });
        }
    });
}


    public void UpdateUserData(string name, string profileImageUrl, int totalScore)
    {
        if (currentUser == null)
        {
            Debug.LogError("[AutenticationID] currentUser is null! Cannot update data.");
            return;
        }

        Debug.Log("[AutenticationID] Updating user data for userId: " + currentUser.UserId);
        DocumentReference userDocRef = db.Collection("users").Document(currentUser.UserId);

        Dictionary<string, object> updatedData = new Dictionary<string, object>
        {
            { "name", name },
            { "profileImageUrl", profileImageUrl },
            { "totalScore", totalScore },
        };

        Debug.Log("[AutenticationID] UpdateUserData called with: name=" + name +
                  ", profileImageUrl=" + profileImageUrl +
                  ", totalScore=" + totalScore);

        userDocRef.UpdateAsync(updatedData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log("[AutenticationID] User data updated successfully in Firestore for userId: " + currentUser.UserId);
            else
                Debug.LogError("[AutenticationID] Error while updating user data in Firestore: " + task.Exception);
        });
    }

    public string GetCharacterState(int totalScore)
    {
        if (totalScore <= 100)
            return "magro";
        else if (totalScore <= 500)
            return "normale";
        else
            return "grosso";
    }
}
