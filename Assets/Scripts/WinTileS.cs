using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinTileS : MonoBehaviour
{
    public string nextLevelName = "Level2";
    public float delayBeforeLoad = 0.2f;
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

            bool isVertical = MoveCube.Instance.IsInVerticalPosition();
            Debug.Log($"[WinTile] IsInVerticalPosition() tras esperar: {isVertical}");
            if (isVertical)
            {
                Debug.Log($"<color=green>¡VICTORIA! El jugador está en posición VERTICAL ✓</color>");
                Debug.Log($"<color=cyan>Cargando nivel: {nextLevelName}</color>");
                MoveCube.Instance.FallIntoHole(nextLevelName);
            }
            else
            {
                Debug.LogWarning($"<color=yellow>El jugador NO está en vertical después de la espera.</color>");
                hasTriggered = false;
            }
        }
    }



}