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
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        AuthenticateAnonymously();
    }

    private async void AuthenticateAnonymously()
    {
        try
        {
            AuthResult result = await auth.SignInAnonymouslyAsync();
            currentUser = result.User;

            if (currentUser != null)
            {
                string userId = currentUser.UserId;
                Debug.Log("Utente anonimo autenticato. ID Utente: " + userId);
                PlayerPrefs.SetString("UserId", userId);
                PlayerPrefs.Save();

                // Sincronizza i dati utente con Firestore
                SyncUserWithFirestore(userId);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Errore durante l'autenticazione anonima: " + ex.Message);
        }
    }

    private void SyncUserWithFirestore(string userId)
    {
        DocumentReference userDocRef = db.Collection("users").Document(userId);

        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                Debug.Log("Dati utente già presenti su Firestore.");
            }
            else
            {
                // Crea nuovi dati di default per l'utente
                Dictionary<string, object> userData = new Dictionary<string, object>
                {
                    { "name", "Nuovo Utente" },
                    { "profileImageUrl", "" },
                    { "totalScore", 0 },
                    { "characterState", "magro" },
                    { "competitors", new List<string>() }
                };

                userDocRef.SetAsync(userData).ContinueWithOnMainThread(saveTask =>
                {
                    if (saveTask.IsCompleted)
                        Debug.Log("Sincronizzazione utente con Firestore completata.");
                    else
                        Debug.LogError("Errore durante la sincronizzazione dei dati.");
                });
            }
        });
    }

    public void UpdateUserData(string name, string profileImageUrl, int totalScore)
    {
        DocumentReference userDocRef = db.Collection("users").Document(currentUser.UserId);

        Dictionary<string, object> updatedData = new Dictionary<string, object>
        {
            { "name", name },
            { "profileImageUrl", profileImageUrl },
            { "totalScore", totalScore },
            { "characterState", GetCharacterState(totalScore) }
        };

        userDocRef.UpdateAsync(updatedData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log("Dati utente aggiornati su Firestore.");
            else
                Debug.LogError("Errore durante l'aggiornamento dei dati.");
        });
    }

    private string GetCharacterState(int totalScore)
    {
        if (totalScore <= 100)
            return "magro";
        else if (totalScore <= 500)
            return "normale";
        else
            return "grosso";
    }
}
