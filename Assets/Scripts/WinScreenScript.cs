using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreenController : MonoBehaviour
{
    public void IrAlMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo...");
        Application.Quit();
    }
}