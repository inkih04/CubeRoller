using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreenController : MonoBehaviour
{
    public void IrAlMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Asegúrate que tu menú se llama así
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo...");
        Application.Quit();
    }
}