using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CloudinaryUploader : MonoBehaviour
{
    // Sostituisci questi valori con quelli ottenuti dal tuo account Cloudinary
    public string cloudName = "dsok7zwnw";         // Il tuo cloud name (lo trovi nel dashboard Cloudinary)
    public string uploadPreset = "my_unsigned_preset";     // Il nome del preset unsigned creato

    /// <summary>
    /// Carica l'immagine su Cloudinary.
    /// </summary>
    /// <param name="imageBytes">I byte dell'immagine (ad esempio, un JPEG o PNG)</param>
    /// <param name="onSuccess">Callback chiamato con l'URL pubblico se l'upload va a buon fine</param>
    /// <param name="onError">Callback chiamato con il messaggio d'errore in caso di fallimento</param>
    public IEnumerator UploadImage(byte[] imageBytes, Action<string> onSuccess, Action<string> onError)
    {
        // Costruisci l'URL di upload per Cloudinary
        string url = $"https://api.cloudinary.com/v1_1/{cloudName}/upload";

        // Crea un form per inviare i dati
        WWWForm form = new WWWForm();
        form.AddField("upload_preset", uploadPreset);
        // Aggiungi l'immagine al form:
        // - "file": nome del campo richiesto da Cloudinary
        // - "profile_image.jpg": nome del file (puoi cambiarlo)
        // - "image/jpeg": MIME type (usa "image/png" se carichi un PNG)
        form.AddBinaryData("file", imageBytes, "profile_image.jpg", "image/jpeg");

        // Crea una richiesta POST per caricare il form
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Errore durante l'upload: " + request.error);
            onError?.Invoke(request.error);
        }
        else
        {
            string responseJson = request.downloadHandler.text;
            Debug.Log("Risposta da Cloudinary: " + responseJson);
            // Deserializza la risposta JSON in un oggetto CloudinaryResponse
            CloudinaryResponse response = JsonUtility.FromJson<CloudinaryResponse>(responseJson);
            if (!string.IsNullOrEmpty(response.secure_url))
            {
                // Richiama il callback di successo passando l'URL pubblico
                onSuccess?.Invoke(response.secure_url);
            }
            else
            {
                onError?.Invoke("secure_url non trovato nella risposta.");
            }
        }
    }
}

/// <summary>
/// Classe per mappare la risposta JSON di Cloudinary.
/// Assicurati che il campo 'secure_url' corrisponda alla risposta di Cloudinary.
/// </summary>
[Serializable]
public class CloudinaryResponse
{
    public string secure_url;
}
