using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinTileS : MonoBehaviour
{
    public string nextLevelName = "Level2";
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

    // Usamos OnTriggerStay para comprobar continuamente mientras el cubo está encima
    void OnTriggerStay(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            MoveCube player = other.GetComponent<MoveCube>();

            // --- DOBLE COMPROBACIÓN DE SEGURIDAD ---
            if (player != null)
            {
                // 1. ¿El jugador ha terminado de moverse? (Evita fallos al pasar rodando)
                bool isStopped = player.IsStopped();

                // 2. ¿Está vertical?
                bool isVertical = player.IsVertical();

                if (isStopped && isVertical)
                {
                    hasTriggered = true;
                    Debug.Log("¡Victoria confirmada! Vertical y Quieto.");
                    player.FallIntoHole(nextLevelName);
                }
            }
        }
    }
}