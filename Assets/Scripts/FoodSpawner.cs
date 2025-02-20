using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject[] goodFoodPrefabs;
    public GameObject[] badFoodPrefabs;
    public Transform spawnPoint;
    public float spawnRate = 1.5f;
    public float minX = -2.5f, maxX = 2.5f;

    private int totalGoodFoodsSpawned = 0;
    private int totalGoodFoodsCollected = 0; // Contatore dei cibi buoni raccolti

    void Start()
    {
        Debug.Log("FoodSpawner avviato correttamente.");
        InvokeRepeating("SpawnFood", 1f, spawnRate);
    }

    void SpawnFood()
    {
        if (totalGoodFoodsSpawned >= 5) // Solo se almeno 5 cibi buoni sono stati generati
        {
            CancelInvoke("SpawnFood");
            Debug.Log("🚫 Spawn Stopped: 5 good foods spawned.");
            return;
        }

        bool spawnGood = (Random.value < 0.5f); // 50% possibilità di spawnare un GoodFood
        GameObject foodPrefab = spawnGood 
            ? goodFoodPrefabs[Random.Range(0, goodFoodPrefabs.Length)] 
            : badFoodPrefabs[Random.Range(0, badFoodPrefabs.Length)];

        if (foodPrefab == null)
        {
            Debug.LogError("ERRORE: Il prefab del cibo è NULL!");
            return;
        }

        Vector3 spawnPos = new Vector3(Random.Range(minX, maxX), spawnPoint.position.y, 0);
        Instantiate(foodPrefab, spawnPos, Quaternion.identity);

        if (spawnGood) totalGoodFoodsSpawned++;
    }

    public void OnGoodFoodCollected()
    {
        totalGoodFoodsCollected++;

        if (totalGoodFoodsCollected >= 5)
        {
            Debug.Log("🎯 5 cibi buoni raccolti! Fine del minigioco.");
            CancelInvoke("SpawnFood"); // Ferma lo spawn
        }
    }
}
