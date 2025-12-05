using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinTileS : MonoBehaviour
{
    public string nextLevelName = "Level2";
    public float delayBeforeLoad = 1f;
    public TMP_Text levelText;
    private bool hasTriggered = false;

    void Start()
    {
        if (levelText != null)
        {
            int currentLevel = SceneManager.GetActiveScene().buildIndex;
            levelText.text = "Level: " + currentLevel;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            // Verificar si el jugador está en posición VERTICAL
            if (MoveCube.Instance != null && MoveCube.Instance.IsInVerticalPosition())
            {
                hasTriggered = true;
                Debug.Log("¡Victoria! El jugador está en posición vertical.");
                Debug.Log("Cargando nivel: " + nextLevelName);
                StartCoroutine(LoadNextLevelWithDelay());
            }
            else
            {
                Debug.Log("El jugador debe estar en posición vertical para ganar.");
            }
        }
    }

    IEnumerator LoadNextLevelWithDelay()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadScene(nextLevelName);
    }
}