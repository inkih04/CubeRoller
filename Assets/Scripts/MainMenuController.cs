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
    public float duracionAnimacion = 0.5f; // Tiempo que tarda en moverse
    public float posicionFueraX = -1500f; // Posición a la izquierda (fuera de pantalla)

    // Referencias privadas a los RectTransforms para moverlos
    private RectTransform rtPrincipal;
    private RectTransform rtCreditos;
    private RectTransform rtNiveles;

    private void Awake()
    {
        // Obtenemos los RectTransform de los paneles automáticamente
        if (panelPrincipal) rtPrincipal = panelPrincipal.GetComponent<RectTransform>();
        if (panelCreditos) rtCreditos = panelCreditos.GetComponent<RectTransform>();
        if (panelNiveles) rtNiveles = panelNiveles.GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Al iniciar, colocamos TODOS fuera (a la izquierda)
        PonerFuera(rtPrincipal);
        PonerFuera(rtCreditos);
        PonerFuera(rtNiveles);

        // Activamos los objetos para que se vean
        if (panelPrincipal) panelPrincipal.SetActive(true);
        if (panelCreditos) panelCreditos.SetActive(true);
        if (panelNiveles) panelNiveles.SetActive(true);

        // Animamos la entrada del menú principal
        StartCoroutine(AnimarEntrada(rtPrincipal));
    }

    // --- FUNCIONES DE BOTONES ---

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

    // Para mantener compatibilidad con tu botón "Volver" genérico si lo usas
    public void VolverMenu()
    {
        // Intenta detectar cual está abierto para cerrarlo (esto es extra por seguridad)
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

    // --- SISTEMA DE ANIMACIÓN ---

    // Mueve un panel de FUERA (-1500) al CENTRO (0)
    IEnumerator AnimarEntrada(RectTransform panel)
    {
        panel.anchoredPosition = new Vector2(posicionFueraX, 0);

        float t = 0;
        Vector2 inicio = panel.anchoredPosition;
        Vector2 fin = Vector2.zero; // El centro de la pantalla

        while (t < 1)
        {
            t += Time.deltaTime / duracionAnimacion;
            // Usamos SmoothStep para que el movimiento sea más suave al final
            panel.anchoredPosition = Vector2.Lerp(inicio, fin, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        panel.anchoredPosition = fin;
    }

    // Mueve el panel actual hacia la izquierda y luego trae el nuevo desde la izquierda
    IEnumerator CambiarPanel(RectTransform panelSaliente, RectTransform panelEntrante)
    {
        // 1. Sacar el panel actual hacia la izquierda (aún más allá o volver a la posición de inicio)
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

        // 2. Esperar un momento (opcional, queda más elegante)
        yield return new WaitForSeconds(0.1f);

        // 3. Meter el panel nuevo desde la izquierda
        yield return StartCoroutine(AnimarEntrada(panelEntrante));
    }

    // Función auxiliar para colocar instantáneamente fuera
    private void PonerFuera(RectTransform rt)
    {
        if (rt != null) rt.anchoredPosition = new Vector2(posicionFueraX, 0);
    }
}