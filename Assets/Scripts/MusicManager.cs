using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    public AudioSource backgroundMusic; // Il componente AudioSource con la musica
    public Button musicToggleButton; // Il bottone nel Main Menu
    public Sprite musicOnSprite; // Immagine quando la musica è attiva
    public Sprite musicOffSprite; // Immagine quando la musica è disattivata

    private bool isMusicOn = true;

    void Awake()
    {
        // Assicura che ci sia solo un MusicManager attivo tra le scene
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Mantiene l'oggetto anche cambiando scena
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Recupera lo stato della musica dai PlayerPrefs
        isMusicOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        UpdateMusicState();
    }

    void Start()
    {
        // Controlla se il bottone è stato assegnato e collega il listener
        if (musicToggleButton != null)
        {
            musicToggleButton.onClick.AddListener(ToggleMusic);
            UpdateButtonIcon(); // Aggiorna l'icona al primo avvio
        }
    }

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("MusicEnabled", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();

        UpdateMusicState();
    }

    void UpdateMusicState()
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.mute = !isMusicOn;
        }
        UpdateButtonIcon();
    }

    void UpdateButtonIcon()
    {
        if (musicToggleButton != null)
        {
            Image buttonImage = musicToggleButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = isMusicOn ? musicOnSprite : musicOffSprite;
            }
        }
    }
}
