using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PushUpGame : MonoBehaviour
{
    [Header("UI Riferimenti")]
    public Button actionButton;
    public Text scoreText;
    public RectTransform forbiddenArea;
    public Canvas canvas;

    [Header("Impostazioni")]
    public int maxPushUps = 15;
    private int pushUpCount = 0;
    private int score = 0;
    private bool canPress = false;

    void Start()
    {
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(OnPressButton);
            actionButton.gameObject.SetActive(false);
        }

        if (canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }

        UpdateScoreText();
    }

    public void OnPushUpDown()
    {
        Debug.Log("Evento push-up!");

        pushUpCount++;

        if (pushUpCount <= maxPushUps)
        {
            MoveButtonToRandomPosition();
            actionButton.gameObject.SetActive(true);
            canPress = true;
        }
        else
        {
            EndGame();
        }
    }

    void OnPressButton()
    {
        if (canPress)
        {
            score++;
            UpdateScoreText();
            actionButton.gameObject.SetActive(false);
            canPress = false;
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Punteggio: " + score;
        }
    }

    void MoveButtonToRandomPosition()
    {
        if (actionButton == null || forbiddenArea == null || canvas == null) return;

        RectTransform buttonRect = actionButton.GetComponent<RectTransform>();
        Vector2 randomPos;
        Vector2 screenPoint;
        int maxAttempts = 100;
        int attempts = 0;

        do
        {
            randomPos = new Vector2(Random.Range(-300, 300), Random.Range(-200, 200));
            screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, canvas.transform.TransformPoint(randomPos));

            attempts++;
            if (attempts > maxAttempts)
            {
                Debug.LogWarning("Impossibile trovare una posizione valida!");
                break;
            }

        } while (RectTransformUtility.RectangleContainsScreenPoint(forbiddenArea, screenPoint, canvas.worldCamera));

        buttonRect.localPosition = randomPos;
    }

    private void EndGame()
    {
        Debug.Log("Minigioco terminato. Torno alla ChallengeScene...");

        // Dopo 5 secondi, torna alla scena Challenge
        Invoke("LoadChallengeScene", 0.1f);
    }

    void LoadChallengeScene()
    {
        SceneManager.LoadScene("ChallengeScene");
    }
}
