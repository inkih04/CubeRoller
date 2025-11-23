using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelPrincipal;
    public GameObject panelCreditos;
    public GameObject panelNiveles;

    // Se llama al pulsar JUGAR
    public void JugarPartida()
    {
        // Carga la siguiente escena en la lista (el Nivel 1)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Se llama al pulsar CREDITOS
    public void AbrirCreditos()
    {
        panelPrincipal.SetActive(false);
        panelCreditos.SetActive(true);
    }

    // Se llama al pulsar SELECCIONAR NIVEL
    public void AbrirNiveles()
    {
        panelPrincipal.SetActive(false);
        panelNiveles.SetActive(true);
    }

    // Se llama al pulsar VOLVER (en cualquier sub-menú)
    public void VolverMenu()
    {
        panelCreditos.SetActive(false);
        panelNiveles.SetActive(false);
        panelPrincipal.SetActive(true);
    }

    // Se llama al pulsar SALIR
    public void SalirJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void CargarNivel(int numeroNivel)
    {
        string nombreEscena = "Level" + numeroNivel;
        // Asegúrate de que la escena exista en Build Settings
        if (Application.CanStreamedLevelBeLoaded(nombreEscena))
        {
            SceneManager.LoadScene(nombreEscena);
        }
        else
        {
            Debug.LogError("La escena " + nombreEscena + " no existe en Build Settings.");
        }
    }
}