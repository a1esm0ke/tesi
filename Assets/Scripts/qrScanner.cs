using UnityEngine;
using UnityEngine.UI;
using ZXing;
using System.Collections.Generic;

public class QRScanner : MonoBehaviour
{
    public RawImage cameraView; // Per visualizzare il feed della fotocamera
    public Text resultText;     // Per mostrare il contenuto del QR Code
    public Text competitorsListText; // Per mostrare la lista dei competitori

    private WebCamTexture webCamTexture; // Per accedere alla fotocamera
    private List<string> competitors = new List<string>(); // Lista dei competitori

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
            // Mostra qualsiasi testo rilevato
            resultText.text = "QR Code rilevato: " + result.Text;
            Debug.Log("QR Code rilevato: " + result.Text);

            // Prova a estrarre l'ID utente (aggiunto per debug)
            string userID = ExtractUserID(result.Text);
            if (!string.IsNullOrEmpty(userID))
            {
                AddCompetitor(userID);
                webCamTexture.Stop();
            }
            else
            {
                Debug.LogWarning("Il QR Code non contiene un ID valido: " + result.Text);
            }
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogWarning("Errore durante la scansione: " + ex.Message);
    }
}


    string ExtractUserID(string qrData)
    {
        // Supponiamo che il QR Code contenga dati del tipo "user:ID12345"
        if (qrData.StartsWith("user:"))
        {
            return qrData.Substring(5); // Rimuove "user:" e restituisce l'ID
        }
        return null; // Restituisce null se il formato non è valido
    }

    void AddCompetitor(string userID)
    {
        if (!competitors.Contains(userID))
        {
            // Aggiungi l'utente alla lista
            competitors.Add(userID);
            Debug.Log("ID utente aggiunto: " + userID);

            // Aggiorna la UI per mostrare la lista dei competitori
            UpdateCompetitorsList();
        }
        else
        {
            Debug.Log("L'utente è già nella lista: " + userID);
        }
    }

    void UpdateCompetitorsList()
    {
        // Crea una stringa con tutti i competitori
        competitorsListText.text = "Competitori:\n";
        foreach (var competitor in competitors)
        {
            competitorsListText.text += "- " + competitor + "\n";
        }
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
