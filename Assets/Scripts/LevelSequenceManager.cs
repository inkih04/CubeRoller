using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSequenceManager : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup blackScreenGroup;
    public TMP_Text levelTitleText;

    [Header("Game References")]
    public LevelMapAnimator levelAnimator;

    [Header("Animation Settings")]
    public float tileDropHeight = 20f;   // Altura desde donde vienen
    public float tileExitDepth = 20f;    // Profundidad a donde van al acabar
    public float tileSpeed = 25f;        // Velocidad
    public float startDelay = 1f;

    private void Start()
    {
        // Setup Inicial
        if (MoveCube.Instance != null) MoveCube.Instance.HidePlayer();

        if (blackScreenGroup != null)
        {
            blackScreenGroup.alpha = 1;
            if (levelTitleText != null) levelTitleText.text = "Nivel " + (SceneManager.GetActiveScene().buildIndex);
        }

        if (levelAnimator != null) levelAnimator.HideLevelInSky(tileDropHeight);

        StartCoroutine(StartLevelSequence());
    }

    IEnumerator StartLevelSequence()
    {
        yield return new WaitForSeconds(startDelay);

        // Fade In Pantalla
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime;
            if (blackScreenGroup != null) blackScreenGroup.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }

        // Mapa cae del cielo
        if (levelAnimator != null)
            yield return StartCoroutine(levelAnimator.AnimateMapFall(tileSpeed));

        // Jugador aparece
        if (MoveCube.Instance != null)
            MoveCube.Instance.SpawnPlayerFromSky(tileDropHeight);
    }

    // --- REINICIAR NIVEL (MUERTE) ---
    public void RestartLevel()
    {
        StartCoroutine(RestartSequence());
    }

    IEnumerator RestartSequence()
    {
        // 1. El mapa se cae al vacío
        if (levelAnimator != null)
            yield return StartCoroutine(levelAnimator.AnimateMapDrop(tileExitDepth, tileSpeed));

        yield return new WaitForSeconds(0.2f); // Pequeña pausa dramática

        // 2. Recargar escena
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // --- SIGUIENTE NIVEL (VICTORIA) ---
    public void LoadNextLevel(string nextLevelName)
    {
        StartCoroutine(NextLevelSequence(nextLevelName));
    }

    IEnumerator NextLevelSequence(string nextLevelName)
    {
        // Esperar a que el jugador termine de caer por el agujero (si es victoria)
        yield return new WaitForSeconds(1f);

        // 1. El mapa se cae al vacío (efecto visual)
        if (levelAnimator != null)
            yield return StartCoroutine(levelAnimator.AnimateMapDrop(tileExitDepth, tileSpeed));

        // 2. Fade a negro
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2;
            if (blackScreenGroup != null) blackScreenGroup.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }

        // 3. Cargar nivel
        SceneManager.LoadScene(nextLevelName);
    }
}