using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelPrincipal;
    public GameObject panelCreditos;
    public GameObject panelNiveles;

 
    public void JugarPartida()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


    public void AbrirCreditos()
    {
        panelPrincipal.SetActive(false);
        panelCreditos.SetActive(true);
    }


    public void AbrirNiveles()
    {
        panelPrincipal.SetActive(false);
        panelNiveles.SetActive(true);
    }


    public void VolverMenu()
    {
        panelCreditos.SetActive(false);
        panelNiveles.SetActive(false);
        panelPrincipal.SetActive(true);
    }


    public void SalirJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void CargarNivel(int numeroNivel)
    {
        string nombreEscena = "Level" + numeroNivel;

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