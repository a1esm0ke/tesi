using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;

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
    private bool scoreUpdated = false; // 🔥 Evita aggiornamenti multipli

    private FirebaseFirestore db;
    private string userId;

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

        db = FirebaseFirestore.DefaultInstance;
        userId = PlayerPrefs.GetString("UserId", "user_example"); // Recupera l'ID utente salvato

        UpdateScoreText();
    }

    public void OnPushUpDown()
    {
        Debug.Log("🏋️‍♂️ Evento push-up!");

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

            // 🔥 Se il punteggio è 15, chiama solo una volta l'aggiornamento
            if (score == 15 && !scoreUpdated)
            {
                scoreUpdated = true; // Blocca ulteriori chiamate
                Debug.Log("🏆 Hai raggiunto 15 punti! Incremento Total Score su Firebase...");
                StartCoroutine(IncrementTotalScoreAndReturn());
            }
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
                Debug.LogWarning("❌ Impossibile trovare una posizione valida!");
                break;
            }

        } while (RectTransformUtility.RectangleContainsScreenPoint(forbiddenArea, screenPoint, canvas.worldCamera));

        buttonRect.localPosition = randomPos;
    }

        private void EndGame()
    {
        Debug.Log($"🎯 Minigioco terminato con {score} punti.");

        if (score == 15 && !scoreUpdated)
        {
            scoreUpdated = true; // 🔥 Evita loop infiniti
            Debug.Log("🔄 Attendere aggiornamento Firebase...");
            StartCoroutine(IncrementTotalScoreAndReturn());
        }
        else
        {
            StartCoroutine(ReturnToGameOverScene());
        }
    }


    IEnumerator ReturnToGameOverScene()
    {
        yield return new WaitForSeconds(0.5f); // Aspetta mezzo secondo
        SceneManager.LoadScene("GameOverScene");
    }

    IEnumerator IncrementTotalScoreAndReturn()
    {
        bool isUpdated = false;
        DocumentReference userRef = db.Collection("users").Document(userId);

        yield return db.RunTransactionAsync(transaction =>
        {
            return transaction.GetSnapshotAsync(userRef).ContinueWith(task =>
            {
                DocumentSnapshot snapshot = task.Result;
                int currentTotalScore = snapshot.Exists && snapshot.TryGetValue<int>("totalScore", out int storedScore) ? storedScore : 0;
                int newTotalScore = currentTotalScore + 1;

                transaction.Update(userRef, "totalScore", newTotalScore);
                Debug.Log($"✅ Total Score aggiornato: {currentTotalScore} → {newTotalScore}");

                isUpdated = true;
                return newTotalScore;
            });
        }).ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("❌ Errore durante l'aggiornamento del Total Score.");
            }
            else
            {
                Debug.Log("✅ Total Score aggiornato con successo su Firebase.");
            }
        });

        if (isUpdated)
        {
            Debug.Log("🏁 Torno alla GameOverScene...");
            StartCoroutine(ReturnToGameOverScene());
        }
    }


}
