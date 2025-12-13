using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 

public class CheatManager : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) CargarNivel("Level1");
        if (Keyboard.current.digit2Key.wasPressedThisFrame) CargarNivel("Level2");
        if (Keyboard.current.digit3Key.wasPressedThisFrame) CargarNivel("Level3");
        if (Keyboard.current.digit4Key.wasPressedThisFrame) CargarNivel("Level4");
        if (Keyboard.current.digit5Key.wasPressedThisFrame) CargarNivel("Level5");
        if (Keyboard.current.digit6Key.wasPressedThisFrame) CargarNivel("Level6");
        if (Keyboard.current.digit7Key.wasPressedThisFrame) CargarNivel("Level7");
        if (Keyboard.current.digit8Key.wasPressedThisFrame) CargarNivel("Level8");
        if (Keyboard.current.digit9Key.wasPressedThisFrame) CargarNivel("Level9");


        if (Keyboard.current.digit0Key.wasPressedThisFrame) CargarNivel("Level10");
    }

    void CargarNivel(string nombreEscena)
    {
        if (Application.CanStreamedLevelBeLoaded(nombreEscena))
        {
            SceneManager.LoadScene(nombreEscena);
        }
        else
        {
            Debug.LogWarning("La escena " + nombreEscena + " no está en Build Settings o no existe.");
        }
    }
}