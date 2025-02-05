using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ZXing;
using Firebase.Firestore;

public class QRScanner : MonoBehaviour
{
    public RawImage cameraView; // Feed della fotocamera
    public Text resultText;     // Per mostrare l'username scansionato
    private WebCamTexture webCamTexture; // Texture per la fotocamera
    private bool isScanning = true;     // Flag per controllare se la scansione è attiva
    private float scanInterval = 0.5f;  // Intervallo tra le scansioni (in secondi)
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

        // Calcola il rapporto d'aspetto
        float aspectRatio = (float)webCamTexture.width / (float)webCamTexture.height;
        cameraView.GetComponent<RectTransform>().sizeDelta = new Vector2(cameraView.GetComponent<RectTransform>().sizeDelta.y * aspectRatio, cameraView.GetComponent<RectTransform>().sizeDelta.y);

        webCamTexture.Play();
        lastScanTime = Time.time;
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

                // Mostra l'ID utente scansionato
                resultText.text = $"QR Code rilevato: {scannedData}";

                // Aggiungi il competitore a Firestore
                AddCompetitorToFirestore(scannedData);

                // Ferma la scansione dopo il rilevamento
                StopScanning();

                // Feedback visivo/sonoro (opzionale)
                Debug.Log($"QR Code rilevato: {scannedData}");
                // AudioSource.PlayClipAtPoint(scanSound, Camera.main.transform.position); // Se hai un suono
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Errore durante la scansione: {ex.Message}");
        }
    }

async void AddCompetitorToFirestore(string competitorId)
{
    string currentUserId = PlayerPrefs.GetString("UserId", "Unknown");
    if (currentUserId == "Unknown")
    {
        Debug.LogError("ID Utente corrente non trovato.");
        return;
    }

    FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
    DocumentReference userRef = db.Collection("users").Document(currentUserId);

    // Aggiorna la lista dei competitori
    await userRef.UpdateAsync("competitors", FieldValue.ArrayUnion(competitorId))
        .ContinueWith(task => {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log($"Competitore aggiunto: {competitorId}");
            }
            else
            {
                Debug.LogError("Errore durante l'aggiunta del competitor a Firestore.");
            }
        });
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