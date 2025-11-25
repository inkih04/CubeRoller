using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinTileS : MonoBehaviour
{
    public string nextLevelName = "Level2";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Game Over!");
            Debug.Log("Siguiente nivel sería: " + nextLevelName);
            SceneManager.LoadScene(nextLevelName);
        }
    }
}
