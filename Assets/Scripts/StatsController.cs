using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Collections.Generic;

public class StatsController : MonoBehaviour
{
    public InputField heightInput;
    public InputField weightInput;
    public InputField ageInput;
    public InputField basalMetabolismInput;
    public InputField trainingDaysInput;
    public InputField bodyFatPercentageInput;

    public Button backButton;
    public Text totalScoreText;

    private FirebaseFirestore db;
    private int previousStatsScore = 0; // 🔥 Memorizza il punteggio delle statistiche precedenti
    private int totalScore = 0; // 🔥 Memorizza il punteggio totale attuale

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        LoadStats();
        LoadTotalScoreFromFirebase(); // 🔥 Recupera il totalScore da Firebase
    }

    public void BackToMainScreen()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void CalculateAndUpdateStats()
    {
        SaveStats();
        UpdateTotalScore();
    }

    private void SaveStats()
    {
        PlayerPrefs.SetString("Height", heightInput.text);
        PlayerPrefs.SetString("Weight", weightInput.text);
        PlayerPrefs.SetString("Age", ageInput.text);
        PlayerPrefs.SetString("BasalMetabolism", basalMetabolismInput.text);
        PlayerPrefs.SetString("TrainingDays", trainingDaysInput.text);
        PlayerPrefs.SetString("BodyFatPercentage", bodyFatPercentageInput.text);
        PlayerPrefs.Save();
    }

    private void LoadStats()
    {
        heightInput.text = PlayerPrefs.GetString("Height", "");
        weightInput.text = PlayerPrefs.GetString("Weight", "");
        ageInput.text = PlayerPrefs.GetString("Age", "");
        basalMetabolismInput.text = PlayerPrefs.GetString("BasalMetabolism", "");
        trainingDaysInput.text = PlayerPrefs.GetString("TrainingDays", "");
        bodyFatPercentageInput.text = PlayerPrefs.GetString("BodyFatPercentage", "");
    }

    private void LoadTotalScoreFromFirebase()
    {
        string userId = PlayerPrefs.GetString("UserId", "UnknownUser");

        if (userId == "UnknownUser")
        {
            Debug.LogError("❌ Errore: UserID non trovato nei PlayerPrefs!");
            return;
        }

        db.Collection("users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                Dictionary<string, object> data = task.Result.ToDictionary();
                totalScore = data.ContainsKey("totalScore") ? Convert.ToInt32(data["totalScore"]) : 0;
                previousStatsScore = data.ContainsKey("statsScore") ? Convert.ToInt32(data["statsScore"]) : 0;

                Debug.Log($"📥 Punteggio totale da Firebase: {totalScore}");
                Debug.Log($"📥 Punteggio Statistiche da Firebase: {previousStatsScore}");
                
                // 🔥 Aggiorna l'UI con il valore corretto
                totalScoreText.text = $"{totalScore}";

                // 🔄 Salva il punteggio nei PlayerPrefs
                PlayerPrefs.SetInt("PlayerTotalScore", totalScore);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogWarning("⚠️ Documento utente non trovato in Firestore.");
            }
        });
    }

    private void UpdateTotalScore()
    {
        try
        {
            float height = float.Parse(heightInput.text);
            float weight = float.Parse(weightInput.text);
            int age = int.Parse(ageInput.text);
            float basalMetabolism = float.Parse(basalMetabolismInput.text);
            int trainingDays = int.Parse(trainingDaysInput.text);
            float bodyFatPercentage = float.Parse(bodyFatPercentageInput.text);

            int newStatsScore = CalculateStatsScore(height, weight, age, bodyFatPercentage, basalMetabolism, trainingDays);

            // 🔥 Rimuoviamo il vecchio StatScore dal totalScore
            totalScore -= previousStatsScore;

            // 🔥 Aggiungiamo il nuovo StatScore
            totalScore += newStatsScore;

            // 🔄 Salviamo il nuovo valore
            PlayerPrefs.SetInt("PlayerTotalScore", totalScore);
            PlayerPrefs.Save();

            // Aggiorna il testo dell'interfaccia utente
            totalScoreText.text = $"{totalScore}";

            // 🔥 Aggiorna Firebase
            UpdateTotalScoreInFirebase(totalScore, newStatsScore);

            // 🔄 Memorizza il nuovo valore per il prossimo aggiornamento
            previousStatsScore = newStatsScore;
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Errore durante il calcolo del punteggio: {e.Message}");
        }
    }

    private int CalculateStatsScore(float height, float weight, int age, float bodyFatPercentage, float basalMetabolism, int trainingDays)
    {
        float bmi = weight / ((height / 100) * (height / 100));
        int score = 0;

        // BMI
        if (bmi < 18.5) score += 20;
        else if (bmi >= 18.5 && bmi < 24.9) score += 50;
        else if (bmi >= 25 && bmi < 30) score += 30;
        else score += 10;

        // Percentuale di grasso corporeo
        if (bodyFatPercentage < 10) score += 30;
        else if (bodyFatPercentage >= 10 && bodyFatPercentage < 20) score += 20;
        else if (bodyFatPercentage >= 20 && bodyFatPercentage < 30) score += 10;

        // Allenamenti settimanali
        score += trainingDays * 5;

        // Metabolismo basale
        if (basalMetabolism > 0 && basalMetabolism <= 1000) score += 10;
        else if (basalMetabolism > 1000 && basalMetabolism <= 2000) score += 20;
        else if (basalMetabolism > 2000) score += 30;

        // Età
        if (age >= 18 && age <= 30) score += 20;
        else if (age > 30 && age <= 50) score += 10;
        else if (age > 50) score += 5;

        return score;
    }

    private void UpdateTotalScoreInFirebase(int totalScore, int statsScore)
    {
        string userId = PlayerPrefs.GetString("UserId", "UnknownUser");

        if (userId == "UnknownUser")
        {
            Debug.LogError("❌ Errore: UserID non trovato nei PlayerPrefs!");
            return;
        }

        Debug.Log($"📤 Aggiornamento totalScore su Firebase per UserID: {userId}, Nuovo Punteggio: {totalScore}, StatScore: {statsScore}");

        db.Collection("users").Document(userId).UpdateAsync(new Dictionary<string, object>
        {
            { "totalScore", totalScore },
            { "statsScore", statsScore }
        }).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"✅ totalScore aggiornato con successo su Firebase! Nuovo valore: {totalScore}");
            }
            else
            {
                Debug.LogError("❌ Errore nell'aggiornamento di totalScore su Firestore: " + task.Exception);
            }
        });
    }
}
