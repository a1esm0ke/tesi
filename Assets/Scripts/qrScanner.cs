using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZXing;

public class QRScanner : MonoBehaviour
{
    public RawImage cameraView; // Feed della fotocamera
    public Text resultText;     // Per mostrare l'username scansionato
    private WebCamTexture webCamTexture; // Texture per la fotocamera

void Start()
{
    webCamTexture = new WebCamTexture();
    cameraView.texture = webCamTexture;
    cameraView.material.mainTexture = webCamTexture;

    // Calcola il rapporto d'aspetto
    float aspectRatio = (float)webCamTexture.width / (float)webCamTexture.height;
    cameraView.GetComponent<RectTransform>().sizeDelta = new Vector2(cameraView.GetComponent<RectTransform>().sizeDelta.y * aspectRatio, cameraView.GetComponent<RectTransform>().sizeDelta.y);

    webCamTexture.Play();
}


    void Update()
    {
        // Esegue la scansione del QR code
        if (webCamTexture.isPlaying)
        {
            ScanQRCode();
        }
    }

    void ScanQRCode()
    {
        try
        {
            // Usa ZXing per leggere il QR code
            IBarcodeReader barcodeReader = new BarcodeReader();
            var color32 = webCamTexture.GetPixels32();
            var result = barcodeReader.Decode(color32, webCamTexture.width, webCamTexture.height);

            if (result != null)
            {
                // Mostra l'username scansionato
                resultText.text = $"QR Code rilevato: {result.Text}";

                // Aggiungi l'utente alla lista globale
                GameData.AddCompetitor(result.Text);

                // Ferma la scansione dopo il rilevamento
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
        // Ferma la fotocamera e torna al menu principale
        webCamTexture.Stop();
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        // Ferma la fotocamera se il GameObject viene distrutto
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
    }
}
