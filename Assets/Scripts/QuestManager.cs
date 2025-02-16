using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Linq;

public class QuestManager : MonoBehaviour 
{
    [System.Serializable]
    public class Exercise
    {
        public string exerciseName;
        public Sprite exerciseImage;
    }

    public List<Exercise> allExercises; // Lista di 10 esercizi totali
    public Image[] exerciseImages; // Le tre immagini casuali per gli esercizi
    public Button[] uploadButtons; // Pulsanti per caricare i video (cliccando l'immagine)
    public GameObject[] checkMarks; // Check verdi per indicare chi ha vinto
    public Toggle[] approvalToggles; // Tre toggle per approvare i video del competitor
    public TMP_Text videoUploadStatusText; // 🔥 Mostra il messaggio "Video caricato"

    public TMP_Text questScoreText; // Punteggio ottenuto dalle quest
    public Button sendDataButton; // Bottone per inviare i kg
    public Button downloadEnemyVideoButton; // Bottone per scaricare i video avversari
    public Button backButton; // Pulsante per tornare alla Challenge Scene

    private FirebaseFirestore db;
    private int completedQuests = 0;
    private Dictionary<int, string> uploadedVideos = new Dictionary<int, string>(); // Salva gli URL caricati
    private List<Exercise> selectedExercises = new List<Exercise>();

    void Start()
    {
    Debug.Log("🎬 Start() eseguito!");

    db = FirebaseFirestore.DefaultInstance;
    backButton.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("ChallengeScene"));
    Debug.Log($"📋 Numero di esercizi in allExercises: {allExercises.Count}");
    foreach (var ex in allExercises)
    {
        Debug.Log($"📌 Esercizio disponibile: {ex.exerciseName} - Sprite: {ex.exerciseImage.name}");
    }

    string lastResetDate = PlayerPrefs.GetString("LastResetDate", "");
    string today = System.DateTime.UtcNow.ToString("yyyy-MM-dd");

    if (lastResetDate != today)
    {
        Debug.Log("📅 Reset settimanale degli esercizi!");
        SelectRandomExercises();
        PlayerPrefs.SetString("LastResetDate", System.DateTime.UtcNow.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();
    }
    else
    {
        Debug.Log("🔄 Recupero esercizi già impostati questa settimana!");
        LoadPreviousExercises();
    }

        // Associa i pulsanti immagine per caricare il video
        for (int i = 0; i < uploadButtons.Length; i++)
        {
            int index = i;
            uploadButtons[i].onClick.AddListener(() => UploadVideo(index));
        }
        
        // Associa il bottone per scaricare i video avversari
        downloadEnemyVideoButton.onClick.AddListener(DownloadEnemyVideos);
        
        sendDataButton.onClick.AddListener(SendApprovalToFirestore);

        // Controlla se ci sono checkmark salvati su Firebase
        LoadCheckmarksFromFirebase();

        UpdateQuestScore();
    }

void LoadPreviousExercises()
{
    Debug.Log("🔍 Tentativo di caricare esercizi precedenti...");

    for (int i = 0; i < exerciseImages.Length; i++)
    {
        if (PlayerPrefs.HasKey($"Exercise_{i}"))
        {
            string exerciseName = PlayerPrefs.GetString($"Exercise_{i}");
            Debug.Log($"🔹 Recuperato da PlayerPrefs: {exerciseName}");

            Exercise foundExercise = allExercises.Find(e => e.exerciseName == exerciseName);

            if (foundExercise != null)
            {
                exerciseImages[i].sprite = foundExercise.exerciseImage;
                exerciseImages[i].color = Color.white; // 🔥 Forza la visibilità dello sprite
                exerciseImages[i].gameObject.SetActive(true);
                Debug.Log($"✅ Assegnata immagine: {foundExercise.exerciseImage.name}");
            }
            else
            {
                Debug.LogError($"❌ Errore: L'esercizio '{exerciseName}' non è stato trovato in allExercises!");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Nessun esercizio salvato per Exercise_{i}");
        }
    }
}




// Seleziona 3 esercizi casuali ogni settimana
void SelectRandomExercises()
{
    Debug.Log("⚡ Selezione di nuovi esercizi settimanali...");

    selectedExercises = allExercises.OrderBy(x => Random.value).Take(3).ToList();

    for (int i = 0; i < selectedExercises.Count; i++)
    {
        Debug.Log($"✅ Selezionato esercizio: {selectedExercises[i].exerciseName}");

        exerciseImages[i].sprite = selectedExercises[i].exerciseImage;
        exerciseImages[i].color = Color.white; // 🔥 Forza la visibilità dello sprite
        exerciseImages[i].gameObject.SetActive(true);

        // 🔥 Salva gli esercizi selezionati
        PlayerPrefs.SetString($"Exercise_{i}", selectedExercises[i].exerciseName);
    }

    PlayerPrefs.Save();
}





    void UploadVideo(int index)
    {
        // Apri la galleria per selezionare il video
        NativeGallery.Permission permission = NativeGallery.GetVideoFromGallery((path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                Debug.Log("Video selezionato: " + path);
                StartCoroutine(UploadToCloudinary(path, index));
            }
            else
            {
                Debug.LogWarning("Nessun video selezionato.");
            }
        });

        if (permission == NativeGallery.Permission.Denied)
        {
            Debug.LogError("Permesso negato per accedere alla galleria.");
        }
    }

IEnumerator UploadToCloudinary(string filePath, int index)
{
    string cloudinaryUrl = "https://api.cloudinary.com/v1_1/dsok7zwnw/video/upload";
    WWWForm form = new WWWForm();
    form.AddField("upload_preset", "my_unsigned_preset");
    form.AddBinaryData("file", System.IO.File.ReadAllBytes(filePath), "video.mp4", "video/mp4");

    using (UnityWebRequest request = UnityWebRequest.Post(cloudinaryUrl, form))
    {
        yield return request.SendWebRequest();

if (request.result == UnityWebRequest.Result.Success)
{
    string responseText = request.downloadHandler.text;
    string videoUrl = ExtractUrlFromResponse(responseText);
    uploadedVideos[index] = videoUrl;
    SaveVideoUrlToFirestore(videoUrl, index);

    // ✅ Mostra la scritta "Video Caricato!"
    videoUploadStatusText.text = "✔ Video caricato!";
    videoUploadStatusText.color = Color.green;

    // ⏳ Nasconde il messaggio dopo 3 secondi
    StartCoroutine(HideUploadStatus());
}


        else
        {
            Debug.LogError("Errore durante l'upload: " + request.error);
        }
    }
}

IEnumerator HideUploadStatus()
{
    yield return new WaitForSeconds(3);
    videoUploadStatusText.text = ""; // 🔥 Cancella il messaggio dopo 3 secondi
}


void SaveVideoUrlToFirestore(string videoUrl, int index)
{
    string userId = PlayerPrefs.GetString("UserId", "UnknownUser"); // 🔥 Controlla il nome corretto dello UserId salvato
    string exerciseKey = "esercizio" + (index + 1); // 🔥 Cambiato per rispettare la tua struttura Firestore

    Debug.Log($"📤 Tentativo di salvataggio Firestore - UserId: {userId}, Key: {exerciseKey}, URL: {videoUrl}");

    if (string.IsNullOrEmpty(userId) || userId == "UnknownUser")
    {
        Debug.LogError("❌ Errore: UserId non trovato in PlayerPrefs!");
        return;
    }

    if (string.IsNullOrEmpty(videoUrl))
    {
        Debug.LogError("❌ Errore: URL del video vuoto!");
        return;
    }

    // 🔥 Stampiamo i dati prima di inviarli per assicurarci che siano giusti
    Dictionary<string, object> data = new Dictionary<string, object>
    {
        { exerciseKey, videoUrl }
    };

    Debug.Log($"📤 Dati inviati a Firestore: {data}");

    db.Collection("users").Document(userId).SetAsync(data, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted)
            Debug.Log($"✅ Video salvato correttamente su Firestore! Campo: {exerciseKey}, URL: {videoUrl}");
        else
            Debug.LogError("❌ Errore nel salvataggio su Firestore: " + task.Exception);
    });
}



    string ExtractUrlFromResponse(string jsonResponse)
    {
        int startIndex = jsonResponse.IndexOf("\"secure_url\":\"") + 14;
        int endIndex = jsonResponse.IndexOf("\"", startIndex);
        return jsonResponse.Substring(startIndex, endIndex - startIndex);
    }

void DownloadEnemyVideos()
{
    string enemyId = PlayerPrefs.GetString("EnemyID", "UnknownEnemy");

    db.Collection("users").Document(enemyId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted)
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();
                for (int i = 0; i < 3; i++)
                {
                    string key = "exercise_" + (i + 1);
                    if (data.ContainsKey(key))
                    {
                        string videoUrl = data[key].ToString();
                        Application.OpenURL(videoUrl); // Apre il video nel browser
                    }
                }
            }
            else
            {
                Debug.LogWarning("Nessun video trovato per l'avversario.");
            }
        }
        else
        {
            Debug.LogError("Errore nel recupero dei video avversari.");
        }
    });
}
void ApproveVideo(int index)
{
    string enemyId = PlayerPrefs.GetString("EnemyID", "UnknownEnemy");
    string checkmarkKey = "checkmark_" + (index + 1);

    db.Collection("users").Document(enemyId).UpdateAsync(new Dictionary<string, object>
    {
        { checkmarkKey, true }
    }).ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted)
        {
            Debug.Log("Checkmark approvato su Firestore.");
            checkMarks[index].SetActive(true);
            LoadCheckmarksFromFirebase(); // 🔥 Aggiunta per aggiornare subito la UI!

        }
        else
        {
            Debug.LogError("Errore nel salvataggio del checkmark.");
        }
    });
}


void LoadCheckmarksFromFirebase()
{
    string userId = PlayerPrefs.GetString("PlayerID", "UnknownUser");

    db.Collection("users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted)
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();
                for (int i = 0; i < checkMarks.Length; i++)
                {
                    string key = "checkmark_" + (i + 1);
                    if (data.ContainsKey(key) && (bool)data[key])
                    {
                        checkMarks[i].SetActive(true);
                    }
                    else
                    {
                        checkMarks[i].SetActive(false);
                    }
                }
            }
        }
    });
}


void SendApprovalToFirestore()
{
    string enemyId = PlayerPrefs.GetString("EnemyID", "UnknownEnemy");
    Dictionary<string, object> approvalData = new Dictionary<string, object>();

    for (int i = 0; i < approvalToggles.Length; i++)
    {
        string checkmarkKey = "checkmark_" + (i + 1);
        approvalData[checkmarkKey] = approvalToggles[i].isOn; // True se l'esercizio è stato approvato
    }

    db.Collection("users").Document(enemyId).UpdateAsync(approvalData).ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted)
        {
            Debug.Log("Approvazione inviata su Firebase.");
            LoadCheckmarksFromFirebase(); // 🔥 Aggiorna la UI dopo aver inviato!

        }
        else
        {
            Debug.LogError("Errore nell'invio dell'approvazione: " + task.Exception);
        }
    });
}


    void UpdateQuestScore()
    {
        questScoreText.text = $"Punteggio Quest: {completedQuests}/3";
    }
}
