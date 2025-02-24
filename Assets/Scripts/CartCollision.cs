using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections;

public class CartCollision : MonoBehaviour
{
    public Text scoreText;
    private int score = 0;
    private int goodFoodCount = 0;
    private FirebaseFirestore db;
    private string userId;
    private FoodSpawner foodSpawner;
    private bool gameEnded = false; // 🔥 Per evitare che il gioco finisca più volte

    [System.Obsolete]
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        userId = PlayerPrefs.GetString("UserId", "unknown_user");
foodSpawner = FindAnyObjectByType<FoodSpawner>(); // ⚡ Più veloce, trova qualsiasi oggetto

        if (foodSpawner != null)
        {
            // ✅ Ascolta l'evento per terminare il gioco quando tutto il cibo è stato droppato
            FoodSpawner.OnAllFoodDropped += EndGame;
        }
        else
        {
            Debug.LogError("❌ FoodSpawner non trovato! Il gioco potrebbe non terminare correttamente.");
        }

        UpdateScore();
    }

    void OnDestroy()
    {
        if (foodSpawner != null)
        {
            // ✅ Rimuove l'evento quando il gioco termina per evitare errori
            FoodSpawner.OnAllFoodDropped -= EndGame;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (gameEnded) return; // 🔥 Se il gioco è già terminato, ignora ulteriori collisioni

        Debug.Log("Il cibo ha toccato qualcosa: " + other.gameObject.name);

        if (other.CompareTag("GoodFood"))
        {
            score += 1;
            goodFoodCount++;
            Debug.Log("🥗 Cibo BUONO raccolto! Punteggio: " + score);

            if (foodSpawner != null)
                foodSpawner.OnGoodFoodCollected();
        }
        else if (other.CompareTag("BadFood"))
        {
            score = Mathf.Max(0, score - 1);
            Debug.Log("❌ Cibo CATTIVO raccolto! Punteggio: " + score);
        }

        other.transform.SetParent(transform, true);
        other.transform.localPosition = new Vector3(0, 0.5f * goodFoodCount, 0);
        Destroy(other.GetComponent<Rigidbody2D>());
        other.GetComponent<Collider2D>().enabled = false;

        UpdateScore();
        CheckGameOver();
    }

    void UpdateScore()
    {
        if (scoreText != null)
        {
            scoreText.text = "Punteggio: " + score;
        }
    }

    void CheckGameOver()
    {
        if (goodFoodCount >= 5)
        {
            Debug.Log("🎯 Gioco Terminato! Punteggio Finale: " + score);
            EndGame();
        }
    }

    void EndGame()
    {
        if (gameEnded) return; // 🔥 Evita che venga chiamato più volte
        gameEnded = true;

        Debug.Log("🎯 Gioco Terminato! Punteggio Finale: " + score);
        StartCoroutine(EndGameCoroutine());
    }

    IEnumerator EndGameCoroutine()
    {
        if (score == 5)
        {
            Debug.Log("🏆 Hai raccolto 5 cibi buoni! Incremento Total Score su Firebase...");
            yield return IncrementTotalScore();
        }

        Debug.Log("⏳ Attendere 3 secondi prima di tornare alla GameOverScene...");
        yield return new WaitForSeconds(3f);

        LoadGameOverScene();
    }

    IEnumerator IncrementTotalScore()
    {
        DocumentReference userRef = db.Collection("users").Document(userId);

        var task = db.RunTransactionAsync(transaction =>
        {
            return transaction.GetSnapshotAsync(userRef).ContinueWith(task =>
            {
                DocumentSnapshot snapshot = task.Result;
                int currentTotalScore = snapshot.Exists && snapshot.TryGetValue<int>("totalScore", out int storedScore) ? storedScore : 0;
                int newTotalScore = currentTotalScore + 1;

                transaction.Update(userRef, "totalScore", newTotalScore);
                Debug.Log($"✅ Total Score aggiornato: {currentTotalScore} → {newTotalScore}");

                return newTotalScore;
            });
        });

        yield return new WaitUntil(() => task.IsCompleted);
    }

    void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOverScene");
    }
}
