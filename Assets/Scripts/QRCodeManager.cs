using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;
using Firebase.Firestore; // Assicurati di aver installato Firebase SDK
using Firebase.Extensions; // Per Task continuations

public class QRCodeManager : MonoBehaviour
{
    public GameObject qrCodePanel; // Riferimento al pannello del QR code
    public RawImage qrCodeImage;   // Riferimento all'immagine del QR code
    public Button openQRPanelButton; // Bottone per aprire il pannello
    public Button backToMainButton;  // Bottone per tornare alla schermata principale

    private FirebaseFirestore db; // Riferimento a Firestore

    private void Start()
    {
        // Inizializza Firestore
        db = FirebaseFirestore.DefaultInstance;

        // Nascondi il pannello del QR code all'avvio
        qrCodePanel.SetActive(false);

        // Assegna gli eventi ai pulsanti
        openQRPanelButton.onClick.AddListener(ShowQRCodePanel);
        backToMainButton.onClick.AddListener(HideQRCodePanel);
    }

    private void ShowQRCodePanel()
    {
            Debug.Log("ShowQRCodePanel() chiamata.");
        // Recupera il codice univoco da Firestore e genera il QR Code
        FetchUniqueCodeFromFirestore((uniqueCode) =>
        {
            if (!string.IsNullOrEmpty(uniqueCode))
            {
                GenerateQRCode(uniqueCode); // Genera il QR Code con il codice univoco
                qrCodePanel.SetActive(true); // Mostra il pannello del QR code
                openQRPanelButton.gameObject.SetActive(false); // Nasconde il bottone
            }
            else
            {
                Debug.LogError("Impossibile recuperare il codice univoco da Firestore.");
            }
        });
    }

    private void HideQRCodePanel()
    {
        // Nasconde il pannello del QR code
        qrCodePanel.SetActive(false);

        // Riabilita e mostra il bottone per generare il QR code
        openQRPanelButton.gameObject.SetActive(true);
    }

    private void GenerateQRCode(string data)
    {
            Debug.Log("GenerateQRCode() chiamata con dati: " + data);

        // Configura il BarcodeWriter per generare un QR code
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = 256, // Altezza del QR code
                Width = 256,  // Larghezza del QR code
                Margin = 1    // Margine intorno al QR code
            }
        };

        try
        {
            // Genera i dati del QR code come array di colori
            Color32[] color32 = writer.Write(data);

            // Crea una nuova Texture2D e applica i dati del QR code
            Texture2D texture = new Texture2D(writer.Options.Width, writer.Options.Height);
            texture.SetPixels32(color32);
            texture.Apply();

            // Assegna la texture all'oggetto RawImage
            qrCodeImage.texture = texture;

            Debug.Log("QR Code generato con dati: " + data);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Errore durante la generazione del QR Code: {ex.Message}");
        }
    }

private void FetchUniqueCodeFromFirestore(System.Action<string> onCodeFetched)
{
    // Recupera l'ID utente salvato in PlayerPrefs
    string userId = PlayerPrefs.GetString("UserId", "Unknown");
    Debug.Log("FetchUniqueCodeFromFirestore() per UserId: " + userId);

    // Usa direttamente l'userId come codice univoco
    onCodeFetched?.Invoke(userId);
}

}