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
            if (currentUser != null)
            {
                string userId = currentUser.UserId;
                Debug.Log("[AutenticationID] Anonymous user authenticated. UserID: " + userId);
                PlayerPrefs.SetString("UserId", userId);
                PlayerPrefs.Save();

                // Sincronizza i dati utente con Firestore
                SyncUserWithFirestore(userId);
            }
            else
            {
                Debug.LogError("[AutenticationID] Authentication succeeded but currentUser is null.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[AutenticationID] Error during anonymous authentication: " + ex.Message);
        }
    }

    public void SyncUserWithFirestore(string userId)
    {
        Debug.Log("[AutenticationID] SyncUserWithFirestore called for userId: " + userId);
        DocumentReference userDocRef = db.Collection("users").Document(userId);

        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                Debug.Log("[AutenticationID] User document already exists in Firestore for userId: " + userId);
            }
            else
            {
                Debug.Log("[AutenticationID] User document not found. Creating new document for userId: " + userId);
                // Crea i dati iniziali per l'utente
                Dictionary<string, object> userData = new Dictionary<string, object>
                {
                    { "name", PlayerPrefs.GetString("PlayerName", "Nuovo Utente") },
                    { "profileImageUrl", "" },
                    { "totalScore", 0 },
                };

                userDocRef.SetAsync(userData).ContinueWithOnMainThread(saveTask =>
                {
                    if (saveTask.IsCompleted)
                        Debug.Log("[AutenticationID] User document created successfully in Firestore for userId: " + userId);
                    else
                        Debug.LogError("[AutenticationID] Error while creating user document in Firestore: " + saveTask.Exception);
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
