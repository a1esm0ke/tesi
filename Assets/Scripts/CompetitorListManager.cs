using UnityEngine;
using UnityEngine.UI;

public class CompetitorListManager : MonoBehaviour
{
    public Transform contentArea; // Dove mostrare gli avversari
    public GameObject competitorPrefab; // Prefab per ogni avversario

    void Start()
    {
        // Puliamo l'area di contenuto
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        // Aggiungiamo ogni competitorio alla lista
        foreach (string competitor in GameData.Competitors)
        {
            GameObject newCompetitor = Instantiate(competitorPrefab, contentArea);
            newCompetitor.GetComponent<Text>().text = competitor;
        }
    }
}
