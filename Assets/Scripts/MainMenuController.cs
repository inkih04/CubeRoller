using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Paneles (Arrastra los GameObjects)")]
    public GameObject panelPrincipal;
    public GameObject panelCreditos;
    public GameObject panelNiveles;

    [Header("Configuración Animación")]
    public float duracionAnimacion = 0.5f; 
    public float posicionFueraX = -1500f; 

    private RectTransform rtPrincipal;
    private RectTransform rtCreditos;
    private RectTransform rtNiveles;

    private void Awake()
    {

        if (panelPrincipal) rtPrincipal = panelPrincipal.GetComponent<RectTransform>();
        if (panelCreditos) rtCreditos = panelCreditos.GetComponent<RectTransform>();
        if (panelNiveles) rtNiveles = panelNiveles.GetComponent<RectTransform>();
    }

    private void Start()
    {
 
        PonerFuera(rtPrincipal);
        PonerFuera(rtCreditos);
        PonerFuera(rtNiveles);


        if (panelPrincipal) panelPrincipal.SetActive(true);
        if (panelCreditos) panelCreditos.SetActive(true);
        if (panelNiveles) panelNiveles.SetActive(true);


        StartCoroutine(AnimarEntrada(rtPrincipal));
    }



    public void JugarPartida()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void AbrirCreditos()
    {
        StartCoroutine(CambiarPanel(rtPrincipal, rtCreditos));
    }

    public void AbrirNiveles()
    {
        StartCoroutine(CambiarPanel(rtPrincipal, rtNiveles));
    }

    public void VolverMenuDesdeCreditos()
    {
        StartCoroutine(CambiarPanel(rtCreditos, rtPrincipal));
    }

    public void VolverMenuDesdeNiveles()
    {
        StartCoroutine(CambiarPanel(rtNiveles, rtPrincipal));
    }


    public void VolverMenu()
    {

        if (rtCreditos.anchoredPosition.x > -100) VolverMenuDesdeCreditos();
        else if (rtNiveles.anchoredPosition.x > -100) VolverMenuDesdeNiveles();
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
            SceneManager.LoadScene(nombreEscena);
        else
            Debug.LogError("La escena " + nombreEscena + " no existe.");
    }


    IEnumerator AnimarEntrada(RectTransform panel)
    {
        panel.anchoredPosition = new Vector2(posicionFueraX, 0);

        float t = 0;
        Vector2 inicio = panel.anchoredPosition;
        Vector2 fin = Vector2.zero; 

        while (t < 1)
        {
            t += Time.deltaTime / duracionAnimacion;

            panel.anchoredPosition = Vector2.Lerp(inicio, fin, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        panel.anchoredPosition = fin;
    }


    IEnumerator CambiarPanel(RectTransform panelSaliente, RectTransform panelEntrante)
    {

        float t = 0;
        Vector2 inicioSalida = panelSaliente.anchoredPosition;
        Vector2 finSalida = new Vector2(posicionFueraX, 0);

        while (t < 1)
        {
            t += Time.deltaTime / duracionAnimacion;
            panelSaliente.anchoredPosition = Vector2.Lerp(inicioSalida, finSalida, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        panelSaliente.anchoredPosition = finSalida;

  
        yield return new WaitForSeconds(0.1f);


        yield return StartCoroutine(AnimarEntrada(panelEntrante));
    }


    private void PonerFuera(RectTransform rt)
    {
        if (rt != null) rt.anchoredPosition = new Vector2(posicionFueraX, 0);
    }
}