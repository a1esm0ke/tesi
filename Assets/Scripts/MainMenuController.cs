using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void OpenQRScanner()
    {
        SceneManager.LoadScene("QRScanner"); // Nome della scena dello scanner QR
    }

    public void OpenStats()
    {
        SceneManager.LoadScene("Stats"); // Nome della scena delle statistiche
    }

    public void OpenCompetitors()
    {
        SceneManager.LoadScene("Competitors"); // Nome della scena dei competitori
    }
}
