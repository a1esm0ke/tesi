using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject[] goodFoodPrefabs;
    public GameObject[] badFoodPrefabs;
    public Transform spawnPoint;
    public float spawnRate = 1.5f;
    public float minX = -2.5f, maxX = 2.5f;

    private int totalGoodFoodsSpawned = 0;

    void Start()
    {Debug.Log("Prefab GoodFood ha Rigidbody2D: " + goodFoodPrefabs[0].GetComponent<Rigidbody2D>());

        Debug.Log("FoodSpawner avviato correttamente.");
        InvokeRepeating("SpawnFood", 1f, spawnRate);
    }

    void SpawnFood()
    {
        if (totalGoodFoodsSpawned >= 5)
        {
            CancelInvoke("SpawnFood");
            Debug.Log("Spawn Stopped: 5 good foods have been spawned.");
            return;
        }

        bool spawnGood = (Random.value < 0.3f);
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
newFood.transform.position = new Vector3(newFood.transform.position.x, newFood.transform.position.y, 0); // Forza Z = 0
Debug.Log("Cibo Spawnato in posizione: " + newFood.transform.position);

        
        if (spawnGood) totalGoodFoodsSpawned++;
    }
}
