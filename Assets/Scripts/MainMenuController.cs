using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuController : MonoBehaviour
{
    public Text playerNameText;               // Riferimento al testo per il nome del giocatore
    public Image playerProfileImage;          // Riferimento all'immagine del profilo del giocatore
    public Sprite defaultProfileSprite;       // Sprite predefinito per il profilo
    public Image characterImage;  
    
   
    private void Start()
    {
        LoadPlayerData();
        //PlayerPrefs.SetInt("PlayerTotalScore", 101);  // Imposta qui il punteggio per testare la seconda skin
        //PlayerPrefs.Save();
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
        SceneManager.LoadScene("CompetitorList");
    }

    private void LoadPlayerData()
    {
        // Carica e mostra il nome del giocatore
        string playerName = PlayerPrefs.GetString("PlayerName", "Campione");
        playerNameText.text = playerName;

        // Carica e mostra la foto del profilo
        string profileUrl = PlayerPrefs.GetString("PlayerProfilePhotoPath", "");
        if (!string.IsNullOrEmpty(profileUrl) && profileUrl.StartsWith("http"))
        {
            // Se il valore è un URL (ottenuto da Cloudinary), scaricalo e mostralo
            StartCoroutine(LoadImageFromURL(profileUrl));
        }
        else if (!string.IsNullOrEmpty(profileUrl) && File.Exists(profileUrl))
        {
            // Se invece è un percorso locale (per test in Editor) lo carica da file
            byte[] imageBytes = File.ReadAllBytes(profileUrl);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageBytes);

            playerProfileImage.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            //AdaptProfileImage();
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

    private System.Collections.IEnumerator LoadImageFromURL(string url)
    {
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(www);
                playerProfileImage.sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
                //AdaptProfileImage();
                Debug.Log("Foto caricata da URL e adattata.");
            }
            else
            {
                Debug.LogError("Errore nel caricamento dell'immagine da URL: " + www.error);
            }
        }
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
        Debug.Log("Nuovo totalScore salvato in PlayerPrefs: " + totalScore);


        UpdateCharacterBasedOnScore(totalScore);

        // Sincronizza i dati su Firebase
        AutenticationID authID = Object.FindAnyObjectByType<AutenticationID>();
        if (authID != null)
        {
            string username = PlayerPrefs.GetString("PlayerName", "Campione");
            string profileImagePath = PlayerPrefs.GetString("PlayerProfilePhotoPath", "");
            authID.UpdateUserData(username, profileImagePath);
            
        }
            else
    {
        Debug.LogError("AutenticationID non trovato durante l'aggiornamento del punteggio.");
    }
    }

    public void SubtractScore(int points)
    {
        int totalScore = PlayerPrefs.GetInt("PlayerTotalScore", 0);
        totalScore = Mathf.Max(0, totalScore - points);  // Evita punteggi negativi

        // Salva il nuovo punteggio
        PlayerPrefs.SetInt("PlayerTotalScore", totalScore);
        PlayerPrefs.Save();

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
            // Leggi i byte dell'immagine selezionata
            byte[] imageBytes = File.ReadAllBytes(path);

            // Mostra l'immagine immediatamente nell'interfaccia utente
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageBytes);
            playerProfileImage.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            //AdaptProfileImage();

            // Utilizza CloudinaryUploader per caricare l'immagine su Cloudinary
            CloudinaryUploader uploader = Object.FindFirstObjectByType<CloudinaryUploader>();
            if (uploader != null)
            {
                StartCoroutine(uploader.UploadImage(imageBytes,
                    (url) =>
                    {
                        Debug.Log("Immagine caricata con successo. URL: " + url);
                        // Salva l'URL in PlayerPrefs
                        PlayerPrefs.SetString("PlayerProfilePhotoPath", url);
                        PlayerPrefs.Save();

                        // Aggiorna Firestore con il nuovo URL
                        AutenticationID authID = Object.FindAnyObjectByType<AutenticationID>();
                        if (authID != null)
                        {
                            string username = PlayerPrefs.GetString("PlayerName", "Campione");
                            int totalScore = PlayerPrefs.GetInt("PlayerTotalScore", 0);
                            authID.UpdateUserData(username, url);
                        }
                    },
                    (error) =>
                    {
                        Debug.LogError("Errore nell'upload dell'immagine: " + error);
                    }));
            }
            else
            {
                Debug.LogError("CloudinaryUploader non trovato. Assicurati di avere un GameObject con questo script nella scena.");
            }
        }
        else
        {
            Debug.LogError("Errore nel caricamento della nuova immagine: file non esistente.");
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
