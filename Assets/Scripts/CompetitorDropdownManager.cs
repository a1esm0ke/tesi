using UnityEngine;
using TMPro; 
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;

public class CompetitorDropdownManager : MonoBehaviour
{
    public TMP_Dropdown competitorDropdown;  // O TMP_Dropdown se usi TextMeshPro
    public Image competitorProfileImage; // Per mostrare l'immagine profilo
    public TMP_Text competitorScoreText;
    public Image competitorCharacterImage; // Per mostrare l'immagine del personaggio
    public Button backButton;         // Bottone per tornare al Main Menu
    public Button challengeButton;  // Riferimento al bottone Challenge

    private FirebaseFirestore db;
    // Una struttura per tenere i dettagli dei competitor (opzionale)
    private Dictionary<string, CompetitorData> competitors = new Dictionary<string, CompetitorData>();

void Start()
{
    db = FirebaseFirestore.DefaultInstance;

    challengeButton.gameObject.SetActive(false); // Nasconde il bottone finché non si sceglie un avversario
    competitorDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    challengeButton.onClick.AddListener(OpenChallengeScene);

    // Gestione del bottone back
    if (backButton != null)
{
    backButton.onClick.AddListener(BackToMainMenu);
    Debug.Log("BackButton assegnato e listener aggiunto.");
}
else
{
    Debug.LogError("BackButton non assegnato nell'Inspector!");
}


    // Imposta l'opzione di default nel dropdown
    competitorDropdown.ClearOptions();
    List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
    options.Add(new TMP_Dropdown.OptionData("Scegli l'avversario"));
    competitorDropdown.AddOptions(options);
    
    HideCompetitorDetails(); // **Nasconde i dettagli all'inizio**

    UpdateCompetitorData();


    // Carica la lista dei competitor salvati
    if (PlayerPrefs.HasKey("ScannedCompetitors"))
    {
        string json = PlayerPrefs.GetString("ScannedCompetitors");
        CompetitorList list = JsonUtility.FromJson<CompetitorList>(json);
        if (list != null && list.competitorIds.Count > 0)
        {
            Debug.Log("Competitor scansionati trovati: " + list.competitorIds.Count);
            // Per ogni competitor, interroga Firebase e aggiungi un'opzione
            List<TMP_Dropdown.OptionData> newOptions = new List<TMP_Dropdown.OptionData>();
            newOptions.Add(new TMP_Dropdown.OptionData("Scegli l'avversario")); // default
            foreach (string compId in list.competitorIds)
            {
                db.Collection("users").Document(compId).GetSnapshotAsync().ContinueWithOnMainThread(task => {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        DocumentSnapshot snapshot = task.Result;
                        if (snapshot.Exists)
                        {
                            Dictionary<string, object> data = snapshot.ToDictionary();
                            string name = data["name"].ToString();
                            string profileImageUrl = data["profileImageUrl"].ToString();
                            int totalScore = int.Parse(data["totalScore"].ToString());
                            
                            CompetitorData compData = new CompetitorData(compId, name, profileImageUrl, totalScore);
                            competitors[name] = compData;
                            
                            newOptions.Add(new TMP_Dropdown.OptionData(name));
                            
                            // Aggiorna il dropdown
                            competitorDropdown.ClearOptions();
                            competitorDropdown.AddOptions(newOptions);
                        }
                        else
                        {
                            Debug.LogError("Documento concorrente non trovato per ID: " + compId);
                        }
                    }
                    else
                    {
                        Debug.LogError("Errore nel recupero per competitor ID: " + compId);
                    }
                });
            }
        }
        else
        {
            Debug.Log("Nessun competitor scansionato da caricare.");
        }
    }
    else
    {
        Debug.Log("Nessuna lista di competitor salvata.");
    }

    competitorDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
}

private void HideCompetitorDetails()
{
    competitorProfileImage.gameObject.SetActive(false);
    competitorCharacterImage.gameObject.SetActive(false);
    competitorScoreText.gameObject.SetActive(false);
}


private void BackToMainMenu()
{
    Debug.Log("Back button premuto! Ritorno al MainMenu.");
    SceneManager.LoadScene("MainMenu");
}



    // Carica la lista degli ID salvati e poi per ciascuno interroga Firestore per ottenere i dati
    void LoadCompetitorsFromPrefs()
    {
        if (PlayerPrefs.HasKey("ScannedCompetitors"))
        {
            string json = PlayerPrefs.GetString("ScannedCompetitors");
            CompetitorList list = JsonUtility.FromJson<CompetitorList>(json);
            if (list != null && list.competitorIds.Count > 0)
            {
                // Prepara le opzioni per il Dropdown
List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

                foreach (string competitorId in list.competitorIds)
                {
                    // Per ogni competitor, recupera i dati da Firestore
                    db.Collection("users").Document(competitorId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted && !task.IsFaulted)
                        {
                            DocumentSnapshot snapshot = task.Result;
                            if (snapshot.Exists)
                            {
                                Dictionary<string, object> data = snapshot.ToDictionary();
                                string name = data["name"].ToString();
                                string profileImageUrl = data["profileImageUrl"].ToString();
                                int totalScore = int.Parse(data["totalScore"].ToString());

                                // Salva i dati in una struttura (CompetitorData, definita in seguito)
                                CompetitorData competitorData = new CompetitorData(competitorId, name, profileImageUrl, totalScore);
                                competitors[competitorId] = competitorData;

                                // Aggiungi l'opzione al Dropdown (in modo asincrono potrebbe non essere in ordine, ma puoi gestirlo)
                                options.Add(new TMP_Dropdown.OptionData(name));

                                // Aggiorna il Dropdown: puoi richiamare un metodo per aggiornare le opzioni
                                UpdateDropdownOptions(options);
                            }
                            else
                            {
                                Debug.LogError("Documento concorrente non trovato per competitorId: " + competitorId);
                            }
                        }
                        else
                        {
                            Debug.LogError("Errore nel recupero del documento per competitorId: " + competitorId);
                        }
                    });
                }
            }
            else
            {
                Debug.Log("Nessun competitor scansionato da caricare.");
            }
        }
        else
        {
            Debug.Log("Nessuna lista di competitor salvata.");
        }
    }

    // Metodo per aggiornare le opzioni del Dropdown
    void UpdateDropdownOptions(List<TMP_Dropdown.OptionData> options)
    {
        competitorDropdown.ClearOptions();
        competitorDropdown.AddOptions(options);
    }

private void UpdateCompetitorData()
{
    Debug.Log("Aggiornamento dati competitor da Firebase...");

    if (PlayerPrefs.HasKey("ScannedCompetitors"))
    {
        string json = PlayerPrefs.GetString("ScannedCompetitors");
        CompetitorList list = JsonUtility.FromJson<CompetitorList>(json);

        if (list != null && list.competitorIds.Count > 0)
        {
            Debug.Log("Trovati " + list.competitorIds.Count + " competitor salvati.");
            competitors.Clear(); // Pulisce la lista precedente
            competitorDropdown.ClearOptions();

            List<TMP_Dropdown.OptionData> newOptions = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("Scegli l'avversario") // Opzione iniziale
            };

            foreach (string compId in list.competitorIds)
            {
                db.Collection("users").Document(compId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        DocumentSnapshot snapshot = task.Result;
                        if (snapshot.Exists)
                        {
                            Dictionary<string, object> data = snapshot.ToDictionary();
                            string name = data["name"].ToString();
                            string profileImageUrl = data["profileImageUrl"].ToString();
                            int totalScore = int.Parse(data["totalScore"].ToString());

                            CompetitorData compData = new CompetitorData(compId, name, profileImageUrl, totalScore);
                            competitors[name] = compData;

                            newOptions.Add(new TMP_Dropdown.OptionData(name));

                            // Aggiorna il dropdown
                            competitorDropdown.ClearOptions();
                            competitorDropdown.AddOptions(newOptions);
                            Debug.Log("Dati aggiornati per " + name);
                        }
                        else
                        {
                            Debug.LogWarning("Documento non trovato per ID: " + compId);
                        }
                    }
                    else
                    {
                        Debug.LogError("Errore nel recupero del documento per ID: " + compId);
                    }
                });
            }
        }
        else
        {
            Debug.Log("Nessun competitor salvato.");
        }
    }
}

    // Gestisci il cambiamento di selezione del Dropdown
private void OnDropdownValueChanged(int index)
{
    if (index == 0) 
    {
        HideCompetitorDetails(); // **Se si seleziona "Scegli un avversario", nasconde tutto**
        challengeButton.gameObject.SetActive(false); // Nasconde il bottone se nessun avversario è selezionato
        return; 
    }
        else
    {
        challengeButton.gameObject.SetActive(true); // Mostra il bottone quando un avversario è selezionato
    }

    string selectedName = competitorDropdown.options[index].text;
    if (competitors.ContainsKey(selectedName))
    {
        CompetitorData compData = competitors[selectedName];
        competitorScoreText.text = "Punti: " + compData.totalScore;

        // **Mostra i dettagli quando un avversario è selezionato**
        competitorProfileImage.gameObject.SetActive(true);
        competitorCharacterImage.gameObject.SetActive(true);
        competitorScoreText.gameObject.SetActive(true);

        StartCoroutine(DownloadAndSetImage(competitorProfileImage, compData.profileImageUrl));

        string characterSpriteName = (compData.totalScore <= 100) ? "Character_Magro" :
                                       (compData.totalScore <= 500) ? "Character_Normale" :
                                                                     "Character_Grosso";

        Sprite characterSprite = Resources.Load<Sprite>("Characters/" + characterSpriteName);
        if (characterSprite != null)
            competitorCharacterImage.sprite = characterSprite;
        else
            Debug.LogError("Sprite per personaggio non trovata: " + characterSpriteName);
    }
}


void OpenChallengeScene()
{
    SceneManager.LoadScene("Challenge");
}


    private IEnumerator DownloadAndSetImage(Image imageComponent, string imageUrl)
    {
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((UnityEngine.Networking.DownloadHandlerTexture)request.downloadHandler).texture;
                imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogError("Errore nel download dell'immagine: " + request.error);
            }
        }
    }
}

// Una semplice struttura per memorizzare i dati del competitor
public class CompetitorData
{
    public string competitorId;
    public string name;
    public string profileImageUrl;
    public int totalScore;

    public CompetitorData(string competitorId, string name, string profileImageUrl, int totalScore)
    {
        this.competitorId = competitorId;
        this.name = name;
        this.profileImageUrl = profileImageUrl;
        this.totalScore = totalScore;
    }

}

