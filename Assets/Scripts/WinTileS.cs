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
            Debug.Log($"[WinTile] Player entró en el trigger.");

            if (MoveCube.Instance == null)
            {
                Debug.LogError("[WinTile] MoveCube.Instance es NULL!");
                return;
            }

            hasTriggered = true;
            StartCoroutine(CheckVerticalAfterDelay());
        }
    }

    IEnumerator CheckVerticalAfterDelay()
    {
        // Esperar 2 segundos antes de comprobar la posición
        yield return new WaitForSeconds(0.7f);

        bool isVertical = MoveCube.Instance.IsInVerticalPosition();
        Debug.Log($"[WinTile] IsInVerticalPosition() tras esperar: {isVertical}");

        if (isVertical)
        {
            Debug.Log($"<color=green>¡VICTORIA! El jugador está en posición VERTICAL ?</color>");
            Debug.Log($"<color=cyan>Cargando nivel: {nextLevelName}</color>");
            StartCoroutine(LoadNextLevelWithDelay());
        }
        else
        {
            Debug.LogWarning($"<color=yellow>El jugador NO está en vertical después de la espera.</color>");
            hasTriggered = false; // Permite reintentar la victoria si vuelve a entrar
        }
    }

    IEnumerator LoadNextLevelWithDelay()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadScene(nextLevelName);
    }
}
