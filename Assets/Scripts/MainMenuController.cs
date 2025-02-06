using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MainMenuController : MonoBehaviour
{
    public Text playerNameText;               // Riferimento al testo per il nome del giocatore
    public Image playerProfileImage;          // Riferimento all'immagine del profilo del giocatore
    public Sprite defaultProfileSprite;       // Sprite predefinito per il profilo

    private string photoFilePath;

    private void Start()
    {
        LoadPlayerData();
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

        // Crea lo sprite senza mantenere il rapporto d'aspetto
        playerProfileImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        // Forza l'adattamento dell'immagine al contenitore, come quando viene caricata una nuova foto
        RectTransform rect = playerProfileImage.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Disattiva il mantenimento delle proporzioni
        playerProfileImage.preserveAspect = false;

        Debug.Log("Foto caricata e adattata.");
    }
    else
    {
        // Se non esiste una foto salvata, usa quella predefinita
        playerProfileImage.sprite = defaultProfileSprite;
        Debug.LogWarning("Nessuna foto trovata, utilizzo immagine di default.");
    }
}


    public void OnProfileImageClick()
    {
        PickImageFromGallery();
    }

    private void PickImageFromGallery()
    {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Scegli una foto", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            Debug.Log("Percorso immagine selezionata: " + path);
            ApplyNewProfileImage(path);
        }
#else
        Debug.Log("Implementa la selezione dell'immagine per piattaforme mobili.");
#endif
    }


private void ApplyNewProfileImage(string path)
{
    if (File.Exists(path))
    {
        byte[] imageBytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);

        // Crea lo sprite senza mantenere il rapporto d'aspetto
        playerProfileImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        // Adatta l'immagine alle dimensioni del contenitore forzando la deformazione
        RectTransform rect = playerProfileImage.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Forza l'aggiornamento dell'immagine disattivando "Preserve Aspect"
        playerProfileImage.preserveAspect = false;

        // Salva il percorso della foto
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


}
