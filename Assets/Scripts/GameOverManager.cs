using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RedirectToChallenge : MonoBehaviour
{
    void Start()
    {
        // Avvia la coroutine che aspetta 3 secondi prima di cambiare scena
        StartCoroutine(WaitAndLoadChallengeScene());
    }

    IEnumerator WaitAndLoadChallengeScene()
    {
        // Aspetta 3 secondi
        yield return new WaitForSeconds(3f);

        // Carica la scena ChallengeScene
        SceneManager.LoadScene("ChallengeScene");
    }
}
