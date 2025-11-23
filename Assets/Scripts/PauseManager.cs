using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Necesario porque usas el nuevo sistema

public class PauseManager : MonoBehaviour
{
    public GameObject panelPausa;
    private bool estaPausado = false;

    void Update()
    {
        // Detectar la tecla Escape usando el teclado actual
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (estaPausado)
                Reanudar();
            else
                Pausar();
        }
    }

    public void Pausar()
    {
        panelPausa.SetActive(true);
        Time.timeScale = 0f; // Congela el tiempo (físicas y movimiento del cubo)
        estaPausado = true;
    }

    public void Reanudar()
    {
        panelPausa.SetActive(false);
        Time.timeScale = 1f; // Devuelve el tiempo a la normalidad
        estaPausado = false;
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f; // Importante: reactivar el tiempo antes de cambiar de escena
        SceneManager.LoadScene("MainMenu"); // Asegúrate que se llama así tu escena
    }
}