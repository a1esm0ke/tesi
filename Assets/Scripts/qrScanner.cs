using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZXing;

public class QRScanner : MonoBehaviour
{
    public RawImage cameraView; // Mostra il feed della fotocamera
    public Text resultText;     // Mostra il risultato del QR Code
    private WebCamTexture webCamTexture; // Per accedere alla fotocamera

    void Start()
    {
        // Inizializza la fotocamera
        webCamTexture = new WebCamTexture();
        cameraView.texture = webCamTexture;
        cameraView.material.mainTexture = webCamTexture;

        // Avvia la fotocamera
        webCamTexture.Play();
    }

    void Update()
    {
        // Se la fotocamera è attiva, prova a scansionare il QR Code
        if (webCamTexture.isPlaying)
        {
            ScanQRCode();
        }
    }

    void ScanQRCode()
    {
        try
        {
            // Usa ZXing per leggere i dati del QR Code
            IBarcodeReader barcodeReader = new BarcodeReader();
            var color32 = webCamTexture.GetPixels32();
            var result = barcodeReader.Decode(color32, webCamTexture.width, webCamTexture.height);

            if (result != null)
            {
                // Mostra il risultato nella UI
                resultText.text = $"QR Code rilevato: {result.Text}";

                // Aggiungi il competitorio alla lista globale
                GameData.AddCompetitor(result.Text);

                // Ferma la fotocamera per evitare doppie scansioni
                webCamTexture.Stop();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Errore durante la scansione: {ex.Message}");
        }
    }

    public void GoBackToMainMenu()
    {
        // Torna al Main Menu
        webCamTexture.Stop(); // Ferma la fotocamera
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        // Ferma la fotocamera quando il GameObject viene distrutto
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
    }
}
