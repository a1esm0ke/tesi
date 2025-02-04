using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterAnimator : MonoBehaviour
{
    private RectTransform characterTransform;
    private bool isAnimating = false;

    void Start()
    {
        characterTransform = GetComponent<RectTransform>();

        // Inizia il loop delle animazioni
        StartCoroutine(AnimateCharacterLoop());
    }

    IEnumerator AnimateCharacterLoop()
    {
        while (true)
        {
            // Attendi tra 3 e 6 secondi prima di un'animazione
            float waitTime = Random.Range(3f, 6f);
            yield return new WaitForSeconds(waitTime);

            // Scegli casualmente tra due animazioni (50% possibilità)
            int randomAnimation = Random.Range(0, 2);

            if (randomAnimation == 0)
            {
                StartCoroutine(NodHeadAnimation()); // Cenno con la testa
            }
            else
            {
                StartCoroutine(WaveHandAnimation()); // Saluto
            }

            // Aspetta la fine dell'animazione prima di ricominciare
            yield return new WaitForSeconds(1.5f);
        }
    }

    IEnumerator NodHeadAnimation()
    {
        if (isAnimating) yield break; // Evita doppie animazioni
        isAnimating = true;

        float duration = 0.5f;
        float elapsedTime = 0f;
        float maxRotation = 10f; // Massimo angolo di inclinazione

        while (elapsedTime < duration)
        {
            float angle = Mathf.Lerp(0, maxRotation, elapsedTime / duration);
            characterTransform.localRotation = Quaternion.Euler(0, 0, angle);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float angle = Mathf.Lerp(maxRotation, 0, elapsedTime / duration);
            characterTransform.localRotation = Quaternion.Euler(0, 0, angle);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        characterTransform.localRotation = Quaternion.identity; // Torna alla posizione originale
        isAnimating = false;
    }

    IEnumerator WaveHandAnimation()
    {
        if (isAnimating) yield break;
        isAnimating = true;

        float duration = 0.5f;
        float elapsedTime = 0f;
        float maxScale = 1.1f; // Il saluto farà "gonfiare" leggermente il personaggio

        while (elapsedTime < duration)
        {
            float scale = Mathf.Lerp(1f, maxScale, elapsedTime / duration);
            characterTransform.localScale = new Vector3(scale, scale, 1);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float scale = Mathf.Lerp(maxScale, 1f, elapsedTime / duration);
            characterTransform.localScale = new Vector3(scale, scale, 1);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        characterTransform.localScale = Vector3.one;
        isAnimating = false;
    }
}
