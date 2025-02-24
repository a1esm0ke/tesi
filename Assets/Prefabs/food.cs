using UnityEngine;

public class Food : MonoBehaviour
{
    private void OnBecameInvisible()
    {
        if (gameObject.CompareTag("GoodFood"))
        {
            Debug.Log("⚠️ Cibo buono perso: " + gameObject.name);
            
            FoodSpawner foodSpawner = FindAnyObjectByType<FoodSpawner>(); // ✅ Soluzione aggiornata
            if (foodSpawner != null)
            {
                foodSpawner.OnGoodFoodMissed();
            }
            else
            {
                Debug.LogError("❌ FoodSpawner non trovato!");
            }
        }
        Destroy(gameObject);
    }
}
