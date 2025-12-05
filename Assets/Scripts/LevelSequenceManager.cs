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
    public float tileDropHeight = 20f;   
    public float tileExitDepth = 20f;   
    public float tileSpeed = 25f;        
    public float startDelay = 1f;

    private void Start()
    {
        // Setup Inicial
        if (MoveCube.Instance != null) MoveCube.Instance.HidePlayer();

        if (blackScreenGroup != null)
        {
            blackScreenGroup.alpha = 1;
            if (levelTitleText != null) levelTitleText.text = "Level " + (SceneManager.GetActiveScene().buildIndex);
        }

        if (levelAnimator != null) levelAnimator.HideLevelInSky(tileDropHeight);

        StartCoroutine(StartLevelSequence());
    }

    IEnumerator StartLevelSequence()
    {
        yield return new WaitForSeconds(startDelay);

 
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime;
            if (blackScreenGroup != null) blackScreenGroup.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }


        if (levelAnimator != null)
            yield return StartCoroutine(levelAnimator.AnimateMapFall(tileSpeed));


        if (MoveCube.Instance != null)
            MoveCube.Instance.SpawnPlayerFromSky(tileDropHeight);
    }


    public void RestartLevel()
    {
        StartCoroutine(RestartSequence());
    }

    IEnumerator RestartSequence()
    {

        if (levelAnimator != null)
            yield return StartCoroutine(levelAnimator.AnimateMapDrop(tileExitDepth, tileSpeed));

        yield return new WaitForSeconds(0.2f); 


        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void LoadNextLevel(string nextLevelName)
    {
        StartCoroutine(NextLevelSequence(nextLevelName));
    }

    IEnumerator NextLevelSequence(string nextLevelName)
    {
        yield return new WaitForSeconds(1f);

        if (levelAnimator != null)
            yield return StartCoroutine(levelAnimator.AnimateMapDrop(tileExitDepth, tileSpeed));

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2;
            if (blackScreenGroup != null) blackScreenGroup.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }

        SceneManager.LoadScene(nextLevelName);
    }
}