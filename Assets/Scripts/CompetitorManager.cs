using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class CompetitorManager : MonoBehaviour
{
    public GameObject competitorCardPrefab;
    public Transform competitorContainer;

    private FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        LoadCompetitorsFromFirestore();
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
        GameObject newCard = Instantiate(competitorCardPrefab, competitorContainer);

        Text nameText = newCard.transform.Find("CompetitorName").GetComponent<Text>();
        Text scoreText = newCard.transform.Find("StatsText").GetComponent<Text>();
        Image profileImage = newCard.transform.Find("ProfileImage").GetComponent<Image>();

        nameText.text = name;
        scoreText.text = "Punti: " + totalScore;

        StartCoroutine(DownloadAndSetImage(profileImage, profileImageUrl));
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
}
