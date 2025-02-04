using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

public class QRCodeManager : MonoBehaviour
{
    public GameObject qrCodePanel; // Riferimento al pannello del QR code
    public RawImage qrCodeImage;   // Riferimento all'immagine del QR code
    public Button openQRPanelButton; // Bottone per aprire il pannello
    public Button backToMainButton;  // Bottone per tornare alla schermata principale

    private void Start()
    {
        // Nascondi il pannello del QR code all'avvio
        qrCodePanel.SetActive(false);

        // Assegna gli eventi ai pulsanti
        openQRPanelButton.onClick.AddListener(ShowQRCodePanel);
        backToMainButton.onClick.AddListener(HideQRCodePanel);
    }

    private void ShowQRCodePanel()
    {
        // Genera il QR code
        GenerateQRCode("Hello World!");

        // Mostra il pannello del QR code
        qrCodePanel.SetActive(true);

        // Nasconde il bottone per generare il QR code
        openQRPanelButton.gameObject.SetActive(false);
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

        // Genera i dati del QR code come array di colori
        Color32[] color32 = writer.Write(data);

        // Crea una nuova Texture2D e applica i dati del QR code
        Texture2D texture = new Texture2D(writer.Options.Width, writer.Options.Height);
        texture.SetPixels32(color32);
        texture.Apply();

        // Assegna la texture all'oggetto RawImage
        qrCodeImage.texture = texture;
    }
}
