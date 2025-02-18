using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Linq;
using System;


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
    public Button[] showVideoButtons; // Assegna i pulsanti nell'Inspector

    public TMP_Text questScoreText; // Punteggio ottenuto dalle quest
    public Button sendDataButton; 
    public Button backButton; // Pulsante per tornare alla Challenge Scene

    private FirebaseFirestore db;
    private int completedQuests = 0;
    private Dictionary<int, string> uploadedVideos = new Dictionary<int, string>(); // Salva gli URL caricati
    private List<Exercise> selectedExercises = new List<Exercise>();

    void Start()
    {
    Debug.Log("🎬 Start() eseguito!");

    db = FirebaseFirestore.DefaultInstance;
    string userId = PlayerPrefs.GetString("UserId", "UnknownUser");
    string enemyId = PlayerPrefs.GetString("EnemyID", "UnknownEnemy");
        // 🔥 Controlliamo se è la fine della settimana
    string lastWeekReset = PlayerPrefs.GetString("LastWeekReset", "");
    string currentWeek = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
    System.DateTime.UtcNow,
    System.Globalization.CalendarWeekRule.FirstFourDayWeek,
    DayOfWeek.Monday
).ToString();
string currentYear = System.DateTime.UtcNow.Year.ToString();
currentWeek = $"{currentYear}-{currentWeek}";


    Debug.Log($"📅 lastWeekReset salvato: {lastWeekReset}");
    Debug.Log($"📅 currentWeek attuale: {currentWeek}");
    if (lastWeekReset != currentWeek)
    {
        Debug.Log("📅 Fine settimana rilevata! Aggiungiamo il punteggio della settimana al totalScore.");
        ResetCheckmarksOnFirebase(); // 🔥 Resetta i checkmark su Firestore
        AddWeeklyScoreToTotal(); // 🔥 Aggiunge il punteggio settimanale al totalScore
        SelectRandomExercises(); // 🔥 Seleziona nuovi esercizi per la settimana
        PlayerPrefs.SetString("LastWeekReset", currentWeek);
        PlayerPrefs.Save();
    }
        else
    {
        Debug.Log("🔄 Recupero esercizi già impostati questa settimana!");
        LoadPreviousExercises();
    }

    backButton.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("ChallengeScene"));
    Debug.Log($"📋 Numero di esercizi in allExercises: {allExercises.Count}");

    // 🔥 Controllo di sicurezza per evitare errori
    if (enemyId == "UnknownEnemy" || userId == "UnknownUser")
    {
        Debug.LogError("❌ Errore: EnemyID o UserID non trovati nei PlayerPrefs!");
       // return; // 🔥 Evitiamo di continuare se i dati non sono validi
    }
    
    foreach (var ex in allExercises)
    {
        Debug.Log($"📌 Esercizio disponibile: {ex.exerciseName} - Sprite: {ex.exerciseImage.name}");
    }
        // Assegna i pulsanti ai metodi di riproduzione
    for (int i = 0; i < showVideoButtons.Length; i++)
    {
        int index = i;
        showVideoButtons[i].onClick.AddListener(() => PlayVideo(index));
    }

        // Associa i pulsanti immagine per caricare il video
        for (int i = 0; i < uploadButtons.Length; i++)
        {
            int index = i;
            uploadButtons[i].onClick.AddListener(() => UploadVideo(index));
        }
        
        ShowEnemyVideos();
        
        sendDataButton.onClick.AddListener(SendApprovalToFirestore);

        // Controlla se ci sono checkmark salvati su Firebase
        LoadCheckmarksFromFirebase();        
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


void PlayVideo(int index)
{
    string enemyId = PlayerPrefs.GetString("EnemyID", "UnknownEnemy");
    string exerciseKey = "esercizio" + (index + 1);

    Debug.Log($"🎥 Tentativo di recuperare il video per {exerciseKey} da Firestore per l'utente: {enemyId}");

    db.Collection("users").Document(enemyId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted)
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Debug.Log("✅ Documento Firestore trovato. Stampiamo i dati...");

                Dictionary<string, object> data = snapshot.ToDictionary();
                foreach (var entry in data)
                {
                    Debug.Log($"📄 Firestore: {entry.Key} -> {entry.Value}");
                }

                if (data.ContainsKey(exerciseKey))
                {
                    string videoUrl = snapshot.GetValue<string>(exerciseKey);
                    Debug.Log($"✅ Video trovato per {exerciseKey}: {videoUrl}");

                    if (!string.IsNullOrEmpty(videoUrl))
                    {
                        Application.OpenURL(videoUrl); // Apre il video con il lettore esterno
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ URL video vuoto per {exerciseKey}");
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠️ Nessun video disponibile per {exerciseKey}");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Documento Firestore non trovato per {enemyId}");
            }
        }
        else
        {
            Debug.LogError($"❌ Errore nel recupero dei dati da Firestore: {task.Exception}");
        }
    });
}



// Seleziona 3 esercizi casuali ogni settimana
void SelectRandomExercises()
{
    Debug.Log("⚡ Selezione di nuovi esercizi settimanali...");

    selectedExercises = allExercises.OrderBy(x => UnityEngine.Random.value).Take(3).ToList();

    for (int i = 0; i < selectedExercises.Count; i++)
    {
        Debug.Log($"✅ Selezionato esercizio: {selectedExercises[i].exerciseName}");

        exerciseImages[i].sprite = selectedExercises[i].exerciseImage;
        exerciseImages[i].color = Color.white; // 🔥 Forza la visibilità dello sprite
        exerciseImages[i].gameObject.SetActive(true);

        // 🔥 Salva gli esercizi selezionati per la settimana
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


private string[] enemyVideoUrls = new string[3]; // Memorizza gli URL dei video ricevuti

void ShowEnemyVideos()
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
                    string key = "esercizio" + (i + 1);
                    
                    if (data.ContainsKey(key))
                    {
                        enemyVideoUrls[i] = data[key].ToString();
                        Debug.Log($"📥 Video {i + 1} ricevuto: {enemyVideoUrls[i]}");
                    }
                    else
                    {
                        enemyVideoUrls[i] = ""; // Nessun video ricevuto
                    }
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Nessun video trovato per l'avversario.");
            }
        }
        else
        {
            Debug.LogError("❌ Errore nel recupero dei video avversari.");
        }
    });
}

void ApproveVideo(int index)
{
    string enemyId = PlayerPrefs.GetString("EnemyID", "UnknownEnemy");
    string myUserId = PlayerPrefs.GetString("UserID", "UnknownUser");

    if (enemyId == "UnknownEnemy" || myUserId == "UnknownUser")
    {
        Debug.LogError("❌ Errore: EnemyID o UserID non trovati nei PlayerPrefs!");
        return;
    }

    string checkmarkKey = $"checkmark_{index + 1}_{myUserId}"; // 🔥 Salva con il tuo ID

    db.Collection("users").Document(enemyId).UpdateAsync(new Dictionary<string, object>
    {
        { checkmarkKey, true }
    }).ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted)
        {
            Debug.Log($"✅ Checkmark {checkmarkKey} approvato su Firestore.");
            checkMarks[index].SetActive(true);
            LoadCheckmarksFromFirebase(); // 🔥 Aggiorna subito la UI
        }
        else
        {
            Debug.LogError("❌ Errore nel salvataggio del checkmark.");
        }
    });
}



void LoadCheckmarksFromFirebase()
{
    string userId = PlayerPrefs.GetString("UserId", "UnknownUser");
    string enemyId = PlayerPrefs.GetString("EnemyID", "UnknownEnemy");

    if (userId == "UnknownUser" || enemyId == "UnknownEnemy")
    {
        Debug.LogError("❌ Errore: EnemyID o UserID non trovati nei PlayerPrefs!");
        return;
    }

    Debug.Log($"🔍 Caricamento checkmark da Firestore per UserID: {userId} con EnemyID: {enemyId}");

    db.Collection("users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted)
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();
                int approvedCount = 0; // 🔄 RESET iniziale

                Debug.Log($"📄 Dati ricevuti da Firestore: {data.Keys.Count} chiavi trovate");

                for (int i = 0; i < checkMarks.Length; i++)
                {
                    string key = $"checkmark_{i + 1}_{enemyId}";

                    if (data.ContainsKey(key))
                    {
                        bool isApproved = Convert.ToBoolean(data[key]);
                        checkMarks[i].SetActive(isApproved);

                        if (isApproved)
                        {
                            approvedCount++;
                            Debug.Log($"✅ Checkmark {i + 1} attivato: {key}");
                        }
                        else
                        {
                            Debug.Log($"❌ Checkmark {i + 1} non attivato: {key}");
                        }
                    }
                    else
                    {
                        checkMarks[i].SetActive(false);
                        Debug.Log($"⚠️ Checkmark {i + 1} non trovato in Firestore.");
                    }
                }

                // ✅ Adesso chiamiamo UpdateQuestScore SOLO dopo aver caricato i dati veri!
                UpdateQuestScore(approvedCount);
            }
            else
            {
                Debug.LogWarning("⚠️ Documento utente non trovato in Firestore.");
            }
        }
        else
        {
            Debug.LogError("❌ Errore nel recupero del documento utente: " + task.Exception);
        }
    });
}



void UpdateTotalScoreOnFirebase(int questScore)
{
    string userId = PlayerPrefs.GetString("UserId", "UnknownUser");
    Debug.Log($"🔍 Tentativo di aggiornare il totalScore per l'utente: {userId}");

    db.Collection("users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted && !task.IsFaulted)
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();
                int currentTotalScore = data.ContainsKey("totalScore") ? Convert.ToInt32(data["totalScore"]) : 0;

                int newTotalScore = currentTotalScore + questScore; // 🔥 Sommiamo i punti delle quest
                Debug.Log($"🔄 Aggiornamento totalScore: {currentTotalScore} + {questScore} = {newTotalScore}");

                db.Collection("users").Document(userId).UpdateAsync(new Dictionary<string, object>
                {
                    { "totalScore", newTotalScore }
                }).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompleted)
                    {
                        Debug.Log("✅ totalScore aggiornato con successo su Firestore.");
                    }
                    else
                    {
                        Debug.LogError("❌ Errore nell'aggiornamento di totalScore su Firestore: " + updateTask.Exception);
                    }
                });
            }
            else
            {
                Debug.LogWarning("⚠️ Documento utente non trovato in Firestore.");
            }
        }
        else
        {
            Debug.LogError("❌ Errore nel recupero del documento utente: " + task.Exception);
        }
    });
}




void SendApprovalToFirestore()
{
    string enemyId = PlayerPrefs.GetString("EnemyID", "UnknownEnemy");
    string userId = PlayerPrefs.GetString("UserId", "UnknownUser");

    if (enemyId == "UnknownEnemy" || userId == "UnknownUser")
    {
        Debug.LogError("❌ Errore: EnemyID o UserID non trovati nei PlayerPrefs!");
        return;
    }

    Dictionary<string, object> approvalData = new Dictionary<string, object>();

    for (int i = 0; i < approvalToggles.Length; i++)
    {
        string checkmarkKey = $"checkmark_{i + 1}_{userId}"; // 🔥 Salviamo con UserID
        approvalData[checkmarkKey] = approvalToggles[i].isOn;
        Debug.Log($"📌 Toggle {i + 1} approvato? {approvalToggles[i].isOn} -> Salvo in: {checkmarkKey}");
    }

    db.Collection("users").Document(enemyId).UpdateAsync(approvalData).ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted)
        {
            Debug.Log("✅ Approvazione inviata su Firebase.");
            LoadCheckmarksFromFirebase(); // 🔥 Aggiorna la UI dopo l'invio
        }
else if (task.IsFaulted)
        {
            Debug.LogError("❌ Errore nell'invio dell'approvazione: " + task.Exception);
        }
        else if (task.IsCanceled)
        {
            Debug.LogWarning("⚠️ La richiesta di aggiornamento è stata annullata.");
        }
        else
        {
            Debug.LogError("❌ Errore sconosciuto: la richiesta non è né completata né fallita.");
        }
    });
}
void UpdateQuestScore(int score)
{
    completedQuests = score; // ✅ Salviamo il punteggio totale

    // 🔥 Sostituiamo invece di accumulare (EVITA errori di doppio punteggio)
    PlayerPrefs.SetInt("WeeklyQuestScore", completedQuests);
    PlayerPrefs.Save();

    questScoreText.text = $"Punteggio Quest: {completedQuests}/3"; // ✅ Aggiorniamo la UI

    Debug.Log($"🎯 Punteggio aggiornato correttamente: {completedQuests}/3");
}

void AddWeeklyScoreToTotal()
{
    string userId = PlayerPrefs.GetString("UserId", "UnknownUser");

    if (userId == "UnknownUser")
    {
        Debug.LogError("❌ Errore: UserID non trovato nei PlayerPrefs!");
        return;
    }

    db.Collection("users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted && !task.IsFaulted)
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();
                int currentTotalScore = data.ContainsKey("totalScore") ? Convert.ToInt32(data["totalScore"]) : 0;
                int totalCheckmarks = 0;

                Debug.Log($"📄 🔍 Scansionando checkmark per userId: {userId}");

                // 🔥 Contiamo tutti i checkmark_x_Y == true (indipendentemente dall'enemyId)
                foreach (var entry in data)
                {
                    if (entry.Key.StartsWith("checkmark_") && entry.Value is bool isTrue && isTrue)
                    {
                        totalCheckmarks++;
                        Debug.Log($"✅ Checkmark attivo: {entry.Key}");
                    }
                }

                Debug.Log($"📊 Checkmark attivi trovati: {totalCheckmarks}");

                int newTotalScore = currentTotalScore + totalCheckmarks;
                Debug.Log($"🔄 PRIMA aggiornamento: totalScore={currentTotalScore}, nuovi punti={totalCheckmarks}, totale aggiornato={newTotalScore}");

                db.Collection("users").Document(userId).UpdateAsync(new Dictionary<string, object>
                {
                    { "totalScore", newTotalScore }
                }).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompleted)
                    {
                        Debug.Log($"✅ totalScore aggiornato con successo su Firestore! Nuovo valore: {newTotalScore}");

                        // 🔥 Resettiamo il punteggio settimanale dopo aver sommato tutto
                        PlayerPrefs.SetInt("WeeklyQuestScore", 0);
                        PlayerPrefs.Save();
                    }
                    else
                    {
                        Debug.LogError("❌ Errore nell'aggiornamento di totalScore su Firestore: " + updateTask.Exception);
                    }
                });
            }
            else
            {
                Debug.LogWarning("⚠️ Documento utente non trovato in Firestore.");
            }
        }
        else
        {
            Debug.LogError("❌ Errore nel recupero del documento utente: " + task.Exception);
        }
    });
}





void ResetCheckmarksOnFirebase()
{
    string userId = PlayerPrefs.GetString("UserId", "UnknownUser");
    string enemyId = PlayerPrefs.GetString("EnemyID", "UnknownEnemy");

    if (userId == "UnknownUser" || enemyId == "UnknownEnemy")
    {
        Debug.LogError("❌ Errore: EnemyID o UserID non trovati nei PlayerPrefs!");
        return;
    }

    Debug.Log("🔄 Reset dei checkmark su Firestore...");

    db.Collection("users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted && task.Result.Exists)
        {
            Dictionary<string, object> data = task.Result.ToDictionary();
            Dictionary<string, object> resetData = new Dictionary<string, object>();

            foreach (var entry in data)
            {
                if (entry.Key.StartsWith("checkmark_")) // Filtra solo i checkmark
                {
                    resetData[entry.Key] = false;
                    Debug.Log($"🔄 Checkmark resettato: {entry.Key}");
                }
            }

            if (resetData.Count > 0)
            {
                db.Collection("users").Document(userId).UpdateAsync(resetData).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompleted)
                    {
                        Debug.Log("✅ Checkmark resettati con successo su Firestore.");
                    }
                    else
                    {
                        Debug.LogError("❌ Errore nel reset dei checkmark su Firestore: " + updateTask.Exception);
                    }
                });
            }
            else
            {
                Debug.Log("⚠️ Nessun checkmark da resettare.");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Documento utente non trovato, nessun reset necessario.");
        }
    });
}



}
