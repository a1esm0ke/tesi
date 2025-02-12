using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class CompetitorManager : MonoBehaviour
{
    public GameObject competitorCardPrefab;
    public Transform competitorContainer;
    public Button backButton; // Aggiungi questo campo per il bottone

    private FirebaseFirestore db;
    
    // Lista per tenere traccia degli ID dei competitor già aggiunti (per evitare duplicati)
    private List<string> addedCompetitors = new List<string>();


void Start()
{
    db = FirebaseFirestore.DefaultInstance;

            // Se esiste il bottone, aggiungi il listener per tornare al Main Menu
        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToMainMenu);
        }
        else
        {
            Debug.LogWarning("BackButton non assegnato nel CompetitorManager.");
        }

    // Carica la lista dei competitor salvati
    if (PlayerPrefs.HasKey("ScannedCompetitors"))
    {
        string json = PlayerPrefs.GetString("ScannedCompetitors");
        CompetitorList list = JsonUtility.FromJson<CompetitorList>(json);

        if (list != null && list.competitorIds.Count > 0)
        {
            Debug.Log("Competitor scansionati trovati: " + list.competitorIds.Count);
            foreach (string competitorId in list.competitorIds)
            {
                AddCompetitorById(competitorId);
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


    private void LoadCompetitorsFromFirestore()
    {
        db.Collection("users").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                foreach (DocumentSnapshot document in task.Result.Documents)
                {
                    Dictionary<string, object> data = document.ToDictionary();

                    string name = data["name"].ToString();
                    string profileImageUrl = data["profileImageUrl"].ToString();
                    int totalScore = int.Parse(data["totalScore"].ToString());

                    CreateCompetitorCard(name, profileImageUrl, totalScore);
                }
            }
            else
            {
                Debug.LogError("Errore nel caricamento degli avversari: " + task.Exception);
            }
        });
    }

private void CreateCompetitorCard(string name, string profileImageUrl, int totalScore)
{
    // Istanzia il prefab come figlio del container
    GameObject newCard = Instantiate(competitorCardPrefab, competitorContainer);
    Debug.Log("CompetitorCard instantiated.");

    // Trova e aggiorna il componente per il nome
    Transform nameTransform = newCard.transform.Find("CompetitorName");
    if (nameTransform != null)
    {
        Text nameText = nameTransform.GetComponent<Text>();
        if (nameText != null)
        {
            nameText.text = name;
            Debug.Log("Name set to: " + name);
        }
        else
        {
            Debug.LogError("Text component not found in 'CompetitorName'.");
        }
    }
    else
    {
        Debug.LogError("Child 'CompetitorName' not found in CompetitorCard prefab.");
    }

    // Trova e aggiorna il componente per il punteggio
    Transform scoreTransform = newCard.transform.Find("StatsText");
    if (scoreTransform != null)
    {
        Text scoreText = scoreTransform.GetComponent<Text>();
        if (scoreText != null)
        {
            scoreText.text = "Punti: " + totalScore;
            Debug.Log("Score set to: " + totalScore);
        }
        else
        {
            Debug.LogError("Text component not found in 'StatsText'.");
        }
    }
    else
    {
        Debug.LogError("Child 'StatsText' not found in CompetitorCard prefab.");
    }

    // Trova e aggiorna il componente per l'immagine del profilo
    Transform profileImageTransform = newCard.transform.Find("ProfileImage");
    if (profileImageTransform != null)
    {
        Image profileImageComp = profileImageTransform.GetComponent<Image>();
        if (profileImageComp != null)
        {
            StartCoroutine(DownloadAndSetImage(profileImageComp, profileImageUrl));
            Debug.Log("Started downloading profile image from: " + profileImageUrl);
        }
        else
        {
            Debug.LogError("Image component not found in 'ProfileImage'.");
        }
    }
    else
    {
        Debug.LogError("Child 'ProfileImage' not found in CompetitorCard prefab.");
    }

    // Trova e aggiorna il componente per l'immagine del personaggio
    Transform characterImageTransform = newCard.transform.Find("CharacterImage");
    if (characterImageTransform != null)
    {
        Image characterImageComp = characterImageTransform.GetComponent<Image>();
        if (characterImageComp != null)
        {
            string characterSpriteName;
            if (totalScore <= 100)
                characterSpriteName = "Character_Magro";
            else if (totalScore <= 500)
                characterSpriteName = "Character_Normale";
            else
                characterSpriteName = "Character_Grosso";

            Sprite characterSprite = Resources.Load<Sprite>("Characters/" + characterSpriteName);
            if (characterSprite != null)
            {
                characterImageComp.sprite = characterSprite;
                Debug.Log("Character image set to: " + characterSpriteName);
            }
            else
            {
                Debug.LogError("Sprite for character not found: " + characterSpriteName);
            }
        }
        else
        {
            Debug.LogError("Image component not found in 'CharacterImage'.");
        }
    }
    else
    {
        Debug.LogError("Child 'CharacterImage' not found in CompetitorCard prefab.");
    }
    LayoutRebuilder.ForceRebuildLayoutImmediate(competitorContainer.GetComponent<RectTransform>());

}



    private IEnumerator<AsyncOperation> DownloadAndSetImage(Image imageComponent, string imageUrl)
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

public void AddCompetitorById(string competitorId)
{
    Debug.Log("AddCompetitorById() chiamata per competitorId: " + competitorId);

    // Controlla se il competitor è già stato aggiunto
    if (addedCompetitors.Contains(competitorId))
    {
        Debug.Log("Competitor già aggiunto: " + competitorId);
        return;
    }
    
    // Aggiungi l'ID alla lista per evitare duplicati futuri
    addedCompetitors.Add(competitorId);
    
    DocumentReference competitorRef = db.Collection("users").Document(competitorId);
    competitorRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
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

                Debug.Log("Dati concorrente recuperati: " + name + ", " + profileImageUrl + ", " + totalScore);
                CreateCompetitorCard(name, profileImageUrl, totalScore);
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

    private void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}


