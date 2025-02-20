using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement; // Importa SceneManager


public class QuizManager : MonoBehaviour
{
    public TMP_Text questionText, timerText;
    public Button[] answerButtons;
    public TMP_Dropdown difficultyDropdown;
    public Button startButton;
    public QuizData easyQuiz, normalQuiz, hardQuiz;
    public GameObject quizUI; // Contiene il box delle domande e i bottoni delle risposte (tutto il blu)
    public GameObject difficultySelectionUI; // Contiene il testo "Scegli la difficoltà", il Dropdown e il bottone Start

    private List<QuizData.Question> currentQuestions;
    private int currentQuestionIndex = 0;
    private int score = 0;
    private float timeRemaining = 600f; // 10 minuti
    private bool isQuizActive = false;

void Start()
{
    startButton.onClick.AddListener(StartQuiz);

    // Mostra solo la selezione della difficoltà, nasconde il quiz
    difficultySelectionUI.SetActive(true);
    quizUI.SetActive(false);
}


void StartQuiz()
{
    if (difficultyDropdown == null)
    {
        Debug.LogError("❌ Errore: Il dropdown della difficoltà non è stato assegnato!");
        return;
    }

    string difficulty = difficultyDropdown.options[difficultyDropdown.value].text;

    currentQuestions = new List<QuizData.Question>();

    switch (difficulty)
    {
        case "Easy":
            if (easyQuiz == null || easyQuiz.questions == null || easyQuiz.questions.Count == 0)
            {
                Debug.LogError("❌ Errore: Nessuna domanda disponibile per la difficoltà Easy!");
                return;
            }
            currentQuestions = new List<QuizData.Question>(easyQuiz.questions);
            break;

        case "Normal":
            if (normalQuiz == null || normalQuiz.questions == null || normalQuiz.questions.Count == 0)
            {
                Debug.LogError("❌ Errore: Nessuna domanda disponibile per la difficoltà Normal!");
                return;
            }
            currentQuestions = new List<QuizData.Question>(normalQuiz.questions);
            break;

        case "Hard":
            if (hardQuiz == null || hardQuiz.questions == null || hardQuiz.questions.Count == 0)
            {
                Debug.LogError("❌ Errore: Nessuna domanda disponibile per la difficoltà Hard!");
                return;
            }
            currentQuestions = new List<QuizData.Question>(hardQuiz.questions);
            break;

        default:
            Debug.LogError("❌ Errore: Difficoltà selezionata non valida!");
            return;
    }

    if (currentQuestions.Count == 0)
    {
        Debug.LogError("❌ Errore: Nessuna domanda trovata per la modalità selezionata!");
        return;
    }

    // ✅ Nasconde il menu di selezione della difficoltà e mostra il quiz
    difficultySelectionUI.SetActive(false);
    quizUI.SetActive(true);

    currentQuestionIndex = 0;
    score = 0;
    isQuizActive = true;

    ShuffleQuestions();
    DisplayQuestion();
    StartCoroutine(TimerCountdown());
}


    void ShuffleQuestions()
    {
        if (currentQuestions == null || currentQuestions.Count == 0)
        {
            Debug.LogError("❌ Errore: Non ci sono domande da mescolare!");
            return;
        }

        for (int i = currentQuestions.Count - 1; i > 0; i--)
        {
            int randIndex = Random.Range(0, i + 1);
            QuizData.Question temp = currentQuestions[i];
            currentQuestions[i] = currentQuestions[randIndex];
            currentQuestions[randIndex] = temp;
        }

        int numQuestions = Mathf.Min(20, currentQuestions.Count);
        currentQuestions = currentQuestions.GetRange(0, numQuestions);

        Debug.Log($"✅ Quiz pronto con {numQuestions} domande mescolate.");
    }

    void DisplayQuestion()
    {
        if (!isQuizActive || currentQuestions == null || currentQuestions.Count == 0)
        {
            Debug.LogError("❌ Errore: Nessuna domanda trovata nella lista!");
            return;
        }

        if (currentQuestionIndex >= currentQuestions.Count)
        {
            EndQuiz();
            return;
        }

        QuizData.Question question = currentQuestions[currentQuestionIndex];

        if (question == null)
        {
            Debug.LogError("❌ Errore: La domanda corrente è null!");
            return;
        }

        Debug.Log($"📝 Mostrando domanda {currentQuestionIndex + 1}: {question.questionText}");

        questionText.text = question.questionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < question.answers.Length)
            {
                TMP_Text answerText = answerButtons[i].GetComponentInChildren<TMP_Text>();
                if (answerText != null)
                {
                    answerText.text = question.answers[i];
                }

                answerButtons[i].gameObject.SetActive(true);
                int answerIndex = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => CheckAnswer(answerIndex));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void CheckAnswer(int index)
    {
        if (!isQuizActive) return;

        if (index == currentQuestions[currentQuestionIndex].correctAnswerIndex)
        {
            score += 10;
            Debug.Log("✅ Risposta corretta!");
        }
        else
        {
            Debug.Log("❌ Risposta sbagliata!");
        }

        currentQuestionIndex++;

        if (currentQuestionIndex < currentQuestions.Count)
        {
            DisplayQuestion();
        }
        else
        {
            EndQuiz();
        }
    }

IEnumerator TimerCountdown()
{
    while (timeRemaining > 0 && isQuizActive)
    {
        timeRemaining -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        // Mostra il tempo nel formato MM:SS
        timerText.text = $"Tempo: {minutes}:{seconds:D2}";

        yield return null;
    }

    EndQuiz();
}


void EndQuiz()
{
    isQuizActive = false;

    // Mostra il punteggio nel formato "Punteggio: X/10"
    questionText.text = $"Quiz terminato!\nPunteggio: {score / 10}/10";
    
    foreach (Button btn in answerButtons)
    {
        btn.gameObject.SetActive(false);
    }

    // Attende 3 secondi e poi torna alla ChallengeScene
    StartCoroutine(ReturnToChallengeScene());
}

// Coroutine che aspetta 3 secondi e cambia scena
IEnumerator ReturnToChallengeScene()
{
    yield return new WaitForSeconds(3f);
    SceneManager.LoadScene("ChallengeScene"); // Nome della scena da caricare
}

}
