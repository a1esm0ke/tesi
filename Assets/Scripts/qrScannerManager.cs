using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZXing;
using System.Collections.Generic;

public class QRScanner : MonoBehaviour
{
    public RawImage cameraView;       // Feed della fotocamera
    public Text resultText;           // Per mostrare l'ID utente scansionato
    public Button backButton;         // Bottone per tornare al Main Menu

    private WebCamTexture webCamTexture; // Texture per la fotocamera
    private bool isScanning = true;       // Flag per controllare se la scansione è attiva
    private float scanInterval = 0.5f;    // Intervallo tra le scansioni (in secondi)
    private float lastScanTime;         // Tempo dell'ultima scansione

    void Start()
    {
        // Inizializza la fotocamera
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("Nessuna fotocamera disponibile.");
            return;
        }

        webCamTexture = new WebCamTexture();
        cameraView.texture = webCamTexture;
        cameraView.material.mainTexture = webCamTexture;
        cameraView.rectTransform.localEulerAngles = new Vector3(0, 0, -webCamTexture.videoRotationAngle + 270);

        // Calcola il rapporto d'aspetto
        float aspectRatio = (float)webCamTexture.width / (float)webCamTexture.height;
        cameraView.GetComponent<RectTransform>().sizeDelta = 
            new Vector2(cameraView.GetComponent<RectTransform>().sizeDelta.y * aspectRatio, 
                        cameraView.GetComponent<RectTransform>().sizeDelta.y);

        webCamTexture.Play();
        lastScanTime = Time.time;

        // Aggiungi il listener al bottone Back (se assegnato)
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBackToMainMenu);
        }
    }

    void Update()
    {
        // Esegue la scansione del QR code solo se è abilitata e rispetta l'intervallo
        if (isScanning && webCamTexture.isPlaying && Time.time - lastScanTime >= scanInterval)
        {
            ScanQRCode();
            lastScanTime = Time.time; // Aggiorna il tempo dell'ultima scansione
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
    string scannedData = result.Text;
    resultText.text = $"QR Code rilevato: {scannedData}";
    Debug.Log($"QR Code rilevato: {scannedData}");

    // Salva l'ID scansionato nella lista persistente
    SaveScannedCompetitor(scannedData);

    // Ferma la scansione
    StopScanning();

    // Carica la scena "Competitors"
    SceneManager.LoadScene("CompetitorList");
}

    }
    catch (System.Exception ex)
    {
        Debug.LogWarning($"Errore durante la scansione: {ex.Message}");
    }
}

private void SaveScannedCompetitor(string competitorId)
{
    CompetitorList list = new CompetitorList();
    if (PlayerPrefs.HasKey("ScannedCompetitors"))
    {
        string json = PlayerPrefs.GetString("ScannedCompetitors");
        if (!string.IsNullOrEmpty(json))
            list = JsonUtility.FromJson<CompetitorList>(json);
    }

    if (!list.competitorIds.Contains(competitorId))
    {
        list.competitorIds.Add(competitorId);
        string newJson = JsonUtility.ToJson(list);
        PlayerPrefs.SetString("ScannedCompetitors", newJson);
        PlayerPrefs.Save();
        Debug.Log("Competitor salvato: " + competitorId + ", JSON: " + newJson);
    }
    else
    {
        Debug.Log("Competitor già presente: " + competitorId);
    }
}





    public void GoBackToMainMenu()
    {
        // Ferma la fotocamera e torna al menu principale
        StopScanning();
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        // Ferma la fotocamera se il GameObject viene distrutto
        StopScanning();
    }

    private void StopScanning()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
            isScanning = false;
        }
    }
}
