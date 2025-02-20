using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections;

public class ChallengeManager : MonoBehaviour
{
    public Button questButton, minigameButton, quizButton;
    public TMP_Text challengeTitle;
    public GameObject questPanel, minigamePanel, quizPanel;
    public Button backButton;
    public Button QuestSceneButton;
    public Button MiniGameSceneButton;
    public Button QuizSceneButton;

    public TMP_Text minigameButtonText;
    public TMP_Text quizButtonText;

    private const string MINI_GAME_KEY = "LastMiniGameTime";
    private const string QUIZ_GAME_KEY = "LastQuizGameTime";

    void Start()
    {
        ShowQuest();

        questButton.onClick.AddListener(ShowQuest);
        minigameButton.onClick.AddListener(ShowMinigame);
        quizButton.onClick.AddListener(ShowQuiz);
        QuestSceneButton.onClick.AddListener(GoToQuestScene);
        //MiniGameSceneButton.onClick.AddListener(AttemptMiniGame);
       //QuizSceneButton.onClick.AddListener(AttemptQuiz);
         MiniGameSceneButton.onClick.AddListener(GoToStartGameScene);
        QuizSceneButton.onClick.AddListener(GoToQuizScene);

        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToMainMenu);
        }
        else
        {
            Debug.LogError("BackButton non assegnato nell'Inspector!");
        }

       // CheckButtonAvailability();
    }

    private void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void GoToStartGameScene()
    {
        SceneManager.LoadScene("StartGameScene");
    }

    private void GoToQuizScene()
    {
        SceneManager.LoadScene("QuizScene");
    }

    public void GoToQuestScene()
    {
        SceneManager.LoadScene("QuestScene");
    }

    void ShowQuest()
    {
        challengeTitle.text = "🏆 Sfida: Quest Settimanale";
        ActivatePanel(questPanel);
    }

    void ShowMinigame()
    {
        challengeTitle.text = "🎮 Sfida: Minigiochi";
        ActivatePanel(minigamePanel);
    }

    void ShowQuiz()
    {
        challengeTitle.text = "📖 Sfida: Quiz";
        ActivatePanel(quizPanel);
    }

    void ActivatePanel(GameObject panelToShow)
    {
        questPanel.SetActive(false);
        minigamePanel.SetActive(false);
        quizPanel.SetActive(false);

        panelToShow.SetActive(true);
    }

    void CheckButtonAvailability()
    {
        DateTime now = DateTime.UtcNow;

        // Controllo per il MiniGame (1 volta al giorno)
        if (PlayerPrefs.HasKey(MINI_GAME_KEY))
        {
            DateTime lastMiniGameTime = DateTime.Parse(PlayerPrefs.GetString(MINI_GAME_KEY));
            double hoursLeft = 24 - (now - lastMiniGameTime).TotalHours;

            if (hoursLeft > 0)
            {
                minigameButton.interactable = false;
                StartCoroutine(UpdateButtonCountdown(minigameButton, minigameButtonText, hoursLeft * 3600, "Minigame"));
            }
            else
            {
                minigameButton.interactable = true;
                minigameButtonText.text = "Minigame";
            }
        }

        // Controllo per il Quiz (1 volta a settimana)
        if (PlayerPrefs.HasKey(QUIZ_GAME_KEY))
        {
            DateTime lastQuizTime = DateTime.Parse(PlayerPrefs.GetString(QUIZ_GAME_KEY));
            double daysLeft = 7 - (now - lastQuizTime).TotalDays;

            if (daysLeft > 0)
            {
                quizButton.interactable = false;
                StartCoroutine(UpdateButtonCountdown(quizButton, quizButtonText, daysLeft * 86400, "Quiz"));
            }
            else
            {
                quizButton.interactable = true;
                quizButtonText.text = "Quiz";
            }
        }
    }

    void AttemptMiniGame()
    {
        if (!minigameButton.interactable)
        {
            Debug.Log("❌ Devi aspettare 24 ore prima di rigiocare il MiniGame!");
            return;
        }

        PlayerPrefs.SetString(MINI_GAME_KEY, DateTime.UtcNow.ToString());
        PlayerPrefs.Save();
        GoToStartGameScene();
    }

    void AttemptQuiz()
    {
        if (!quizButton.interactable)
        {
            Debug.Log("❌ Devi aspettare 7 giorni prima di rigiocare il Quiz!");
            return;
        }

        PlayerPrefs.SetString(QUIZ_GAME_KEY, DateTime.UtcNow.ToString());
        PlayerPrefs.Save();
        GoToQuizScene();
    }

    IEnumerator UpdateButtonCountdown(Button button, TMP_Text buttonText, double totalSeconds, string defaultText)
    {
        while (totalSeconds > 0)
        {
            totalSeconds -= Time.deltaTime;

            TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
            if (timeSpan.TotalHours >= 1)
            {
                buttonText.text = $"{(int)timeSpan.TotalHours}h {(int)timeSpan.Minutes}m";
            }
            else
            {
                buttonText.text = $"{(int)timeSpan.Minutes}m {(int)timeSpan.Seconds}s";
            }

            yield return null;
        }

        button.interactable = true;
        buttonText.text = defaultText;
    }
}
