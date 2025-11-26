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
            levelText.text = "Level: " + currentLevel ;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            Debug.Log("¡Victoria!");
            Debug.Log("Cargando nivel: " + nextLevelName);
            StartCoroutine(LoadNextLevelWithDelay());
        }
    }

    IEnumerator LoadNextLevelWithDelay()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadScene(nextLevelName);
    }
}