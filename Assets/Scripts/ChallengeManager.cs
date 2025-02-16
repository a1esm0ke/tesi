using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ChallengeManager : MonoBehaviour
{
    public Button questButton, minigameButton, quizButton; // Pulsanti per cambiare modalità
    public TMP_Text challengeTitle; // Testo che indica la modalità selezionata
    public GameObject questPanel, minigamePanel, quizPanel; // Pannelli delle modalità
    public Button backButton;         // Bottone per tornare al Main Menu
    public Button QuestSceneButton; 
    void Start()
    {
        // Imposta di default la modalità "Quest"
        ShowQuest();

        // Assegna le funzioni ai pulsanti
        questButton.onClick.AddListener(ShowQuest);
        minigameButton.onClick.AddListener(ShowMinigame);
        quizButton.onClick.AddListener(ShowQuiz);
        QuestSceneButton.onClick.AddListener(GoToQuestScene);
            // Gestione del bottone back
    if (backButton != null)
{
    backButton.onClick.AddListener(BackToMainMenu);
    Debug.Log("BackButton assegnato e listener aggiunto.");
}
else
{
    Debug.LogError("BackButton non assegnato nell'Inspector!");
}
    }



private void BackToMainMenu()
{
    Debug.Log("Back button premuto! Ritorno al MainMenu.");
    SceneManager.LoadScene("MainMenu");
}

private void GoToQuestScene()
{
    Debug.Log("Quest scene button premuto! Vai alla scena quest.");
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
}
