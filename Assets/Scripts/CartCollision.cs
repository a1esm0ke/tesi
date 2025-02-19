using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CartCollision : MonoBehaviour
{
    public Text scoreText; // Riferimento alla UI
    private int score = 0;
    private int goodFoodCount = 0; // Conta solo i cibi buoni raccolti

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Il cibo ha toccato qualcosa: " + other.gameObject.name);

        if (other.CompareTag("GoodFood"))
        {
            score += 1; // Aggiunge 1 punto per ogni GoodFood
            goodFoodCount++; // Conta i cibi buoni raccolti
            Debug.Log("Cibo BUONO raccolto! Punteggio: " + score);
            
            // Attacca il cibo al carrello e posizionalo sopra
            other.transform.parent = transform;
            other.transform.position = transform.position + new Vector3(0, 0.5f, 0);
            
            // Rimuove il Rigidbody2D per evitare movimenti strani
            Destroy(other.GetComponent<Rigidbody2D>());
            
            // Disabilita il Collider2D per evitare collisioni multiple
            other.GetComponent<Collider2D>().enabled = false;
        }
else if (other.CompareTag("BadFood"))
{
    Debug.Log("Cibo CATTIVO raccolto! (Nessun punto assegnato)");

    // Attacca il cibo cattivo al carrello e posizionalo sopra
    other.transform.parent = transform;
    other.transform.position = transform.position + new Vector3(0, 0.5f, 0);

    // Rimuove il Rigidbody2D per evitare movimenti strani
    Destroy(other.GetComponent<Rigidbody2D>());

    // Disabilita il Collider2D per evitare collisioni multiple
    other.GetComponent<Collider2D>().enabled = false;
}


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
        if (goodFoodCount >= 5) // Se ha preso 5 cibi buoni, termina il gioco
        {
            Debug.Log("🎯 Gioco Terminato! Punteggio Finale: " + score);
            Invoke("LoadChallengeScene", 2f); // Aspetta 2 secondi e torna alla ChallengeScene
        }
    }

    void LoadChallengeScene()
    {
        SceneManager.LoadScene("ChallengeScene");
    }
}
