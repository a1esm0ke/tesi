using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ChallengeManager : MonoBehaviour
{
    public Button questButton, minigameButton, quizButton; // Pulsanti per cambiare modalità
    public TMP_Text challengeTitle; // Testo che indica la modalità selezionata
    public GameObject questPanel, minigamePanel, quizPanel; // Pannelli delle modalità

    void Start()
    {
        // Imposta di default la modalità "Quest"
        ShowQuest();

        // Assegna le funzioni ai pulsanti
        questButton.onClick.AddListener(ShowQuest);
        minigameButton.onClick.AddListener(ShowMinigame);
        quizButton.onClick.AddListener(ShowQuiz);
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
