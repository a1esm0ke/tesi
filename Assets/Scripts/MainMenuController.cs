using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuController : MonoBehaviour
{
    public Text playerNameText;               // Riferimento al testo per il nome del giocatore
    public Image playerProfileImage;          // Riferimento all'immagine del profilo del giocatore
    public Sprite defaultProfileSprite;       // Sprite predefinito per il profilo
    public Image characterImage;              // Riferimento all'immagine del personaggio
    private string photoFilePath;

    private void Start()
    {
        LoadPlayerData();
        PlayerPrefs.SetInt("PlayerTotalScore", 101);  // Imposta qui il punteggio per testare la seconda skin
        PlayerPrefs.Save();
    }

    public void OpenQRScanner()
    {
        SceneManager.LoadScene("QRScanner");
    }

    public void OpenStats()
    {
        SceneManager.LoadScene("Stats");
    }

    public void OpenCompetitors()
    {
        SceneManager.LoadScene("Competitors");
    }

    private void LoadPlayerData()
    {
        // Carica e mostra il nome del giocatore
        string playerName = PlayerPrefs.GetString("PlayerName", "Campione");
        playerNameText.text = playerName;

        // Carica e mostra la foto del profilo
        photoFilePath = PlayerPrefs.GetString("PlayerProfilePhotoPath", "");

        if (File.Exists(photoFilePath))
        {
            byte[] imageBytes = File.ReadAllBytes(photoFilePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageBytes);

            playerProfileImage.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));

            AdaptProfileImage();
            Debug.Log("Foto caricata e adattata.");
        }
        else
        {
            playerProfileImage.sprite = defaultProfileSprite;
            Debug.LogWarning("Nessuna foto trovata, utilizzo immagine di default.");
        }

        // Carica e aggiorna il personaggio basato sul punteggio totale
        int totalScore = PlayerPrefs.GetInt("PlayerTotalScore", 0);
        UpdateCharacterBasedOnScore(totalScore);
    }

    private void UpdateCharacterBasedOnScore(int totalScore)
    {
        if (characterImage == null)
        {
            Debug.LogError("characterImage non è assegnato. Controlla il riferimento nello script.");
            return;
        }

        string characterSpriteName;

        if (totalScore <= 100)
            characterSpriteName = "Character_Magro";     // Magro
        else if (totalScore <= 500)
            characterSpriteName = "Character_Normale";   // Normale
        else
            characterSpriteName = "Character_Grosso";    // Grosso

        Sprite characterSprite = Resources.Load<Sprite>("Characters/" + characterSpriteName);

        if (characterSprite != null)
        {
            characterImage.sprite = characterSprite;
            Debug.Log($"Personaggio aggiornato: {characterSpriteName} (Punteggio: {totalScore})");
        }
        else
        {
            Debug.LogError($"Sprite del personaggio non trovato: {characterSpriteName}. Verifica il percorso e il nome dello sprite.");
        }
    }

    public void AddScore(int points)
    {
        int totalScore = PlayerPrefs.GetInt("PlayerTotalScore", 0);
        totalScore += points;

        // Salva il nuovo punteggio
        PlayerPrefs.SetInt("PlayerTotalScore", totalScore);
        PlayerPrefs.Save();

        // Aggiorna l'immagine del personaggio
        UpdateCharacterBasedOnScore(totalScore);
    }

    public void SubtractScore(int points)
    {
        int totalScore = PlayerPrefs.GetInt("PlayerTotalScore", 0);
        totalScore = Mathf.Max(0, totalScore - points);  // Evita punteggi negativi

        // Salva il nuovo punteggio
        PlayerPrefs.SetInt("PlayerTotalScore", totalScore);
        PlayerPrefs.Save();

        // Aggiorna l'immagine del personaggio
        UpdateCharacterBasedOnScore(totalScore);
    }

    public void OnProfileImageClick()
    {
        PickImageFromGallery();
    }

    private void PickImageFromGallery()
    {
#if UNITY_ANDROID
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                Debug.Log("Percorso immagine selezionata: " + path);
                ApplyNewProfileImage(path);
            }
            else
            {
                Debug.LogWarning("Nessuna immagine selezionata.");
            }
        }, "Scegli una foto per il profilo", "image/*");

        if (permission == NativeGallery.Permission.Denied)
        {
            Debug.LogError("Permesso negato per accedere alla galleria.");
        }
#elif UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Scegli una foto", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log("Percorso immagine selezionata: " + path);
            ApplyNewProfileImage(path);
        }
        else
        {
            Debug.LogWarning("Nessuna immagine selezionata.");
        }
#else
        Debug.Log("Implementa la selezione dell'immagine per altre piattaforme.");
#endif
    }

    private void ApplyNewProfileImage(string path)
    {
        if (File.Exists(path))
        {
            byte[] imageBytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageBytes);

            playerProfileImage.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));

            AdaptProfileImage();

            photoFilePath = Path.Combine(Application.persistentDataPath, "PlayerProfile.png");
            File.WriteAllBytes(photoFilePath, imageBytes);
            PlayerPrefs.SetString("PlayerProfilePhotoPath", photoFilePath);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogError("Errore nel caricamento della nuova immagine.");
        }
    }

    private void AdaptProfileImage()
    {
        RectTransform rect = playerProfileImage.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        playerProfileImage.preserveAspect = false;
    }
}
