using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject[] goodFoodPrefabs;
    public GameObject[] badFoodPrefabs;
    public Transform spawnPoint;
    public float spawnRate = 1.5f;
    public float minX = -2f, maxX = 2f;

    private int totalGoodFoodsSpawned = 0;
    private int totalGoodFoodsCollected = 0;
    private int activeGoodFoods = 0; // 🔥 Tiene traccia dei cibi buoni ancora in gioco

public delegate void FoodDroppedEvent();
public static event FoodDroppedEvent OnAllFoodDropped;


    void Start()
    {
        Debug.Log("FoodSpawner avviato correttamente.");
        InvokeRepeating("SpawnFood", 1f, spawnRate);
    }

    void SpawnFood()
    {
        if (totalGoodFoodsSpawned >= 5)
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
        GameObject newFood = Instantiate(foodPrefab, spawnPos, Quaternion.identity);

        if (spawnGood)
        {
            totalGoodFoodsSpawned++;
            activeGoodFoods++; // 🔥 Aumenta il numero di cibi buoni in gioco
        }
    }

    public void OnGoodFoodCollected()
    {
        totalGoodFoodsCollected++;
        activeGoodFoods--; // 🔥 Riduce il numero di cibi buoni in gioco

        if (totalGoodFoodsCollected >= 5)
        {
            Debug.Log("🎯 5 cibi buoni raccolti! Fine del minigioco.");
            CancelInvoke("SpawnFood");
            CheckAllFoodDropped();
        }
    }

    public void OnGoodFoodMissed()
    {
        activeGoodFoods--; // 🔥 Un cibo buono è stato perso
        CheckAllFoodDropped();
    }

private void CheckAllFoodDropped()
{
    Debug.Log($"📊 Controllo fine gioco: Spawnati: {totalGoodFoodsSpawned}, Attivi: {activeGoodFoods}");

    if (totalGoodFoodsSpawned >= 5 && activeGoodFoods <= 0)
    {
        Debug.Log("🎯 Tutti i cibi buoni sono stati raccolti o persi. Fine del minigioco.");
        OnAllFoodDropped?.Invoke();
    }
}

}
