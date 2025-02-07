using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StatsController : MonoBehaviour
{
    public InputField heightInput;
    public InputField weightInput;
    public InputField ageInput;
    public InputField basalMetabolismInput;
    public InputField trainingDaysInput;
    public InputField bodyFatPercentageInput;

    public Text weeklyScoreText;
    public Button backButton;
    public Text totalScoreText;

    private int weeklyScore;

    void Start()
    {
        LoadWeeklyScore();
        LoadStats();
        LoadTotalScore();
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

    private void UpdateTotalScore()
    {
        float height = float.Parse(heightInput.text);
        float weight = float.Parse(weightInput.text);
        int age = int.Parse(ageInput.text);
        float basalMetabolism = float.Parse(basalMetabolismInput.text);
        int trainingDays = int.Parse(trainingDaysInput.text);
        float bodyFatPercentage = float.Parse(bodyFatPercentageInput.text);

        int statsScore = CalculateStatsScore(height, weight, age, bodyFatPercentage, basalMetabolism, trainingDays);
        int totalScore = statsScore + weeklyScore;

        // Salva il punteggio totale nei PlayerPrefs
        PlayerPrefs.SetInt("PlayerTotalScore", totalScore);
        PlayerPrefs.Save();

        // Aggiorna il testo delle statistiche
        totalScoreText.text = $"{totalScore}";
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

    private void SaveWeeklyScore()
    {
        PlayerPrefs.SetInt("WeeklyScore", weeklyScore);
        PlayerPrefs.Save();
    }

    private void LoadWeeklyScore()
    {
        weeklyScore = PlayerPrefs.GetInt("WeeklyScore", 0);
        weeklyScoreText.text = $"{weeklyScore}";
    }

    private void LoadTotalScore()
    {
        int totalScore = PlayerPrefs.GetInt("PlayerTotalScore", 0);
        totalScoreText.text = $"{totalScore}";
    }
}
