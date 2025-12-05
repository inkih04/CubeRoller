using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinTileS : MonoBehaviour
{
    public string nextLevelName = "Level2";
    public float delayBeforeLoad = 1f;
    public TMP_Text levelText;

    private bool hasTriggered = false;

    void Start()
    {
        if (levelText != null)
        {
            int currentLevel = SceneManager.GetActiveScene().buildIndex;
            levelText.text = "Level: " + currentLevel;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            // Verificar si el jugador está en posición vertical
            if (IsPlayerVertical(other.transform))
            {
                hasTriggered = true;
                Debug.Log("¡Victoria! - Jugador en posición vertical");
                Debug.Log("Cargando nivel: " + nextLevelName);
                StartCoroutine(LoadNextLevelWithDelay());
            }
            else
            {
                Debug.Log("El jugador debe estar en posición vertical para ganar");
            }
        }
    }

    bool IsPlayerVertical(Transform player)
    {
        // Obtener el BoxCollider del jugador
        BoxCollider boxCollider = player.GetComponent<BoxCollider>();

        if (boxCollider == null)
        {
            Debug.LogWarning("El jugador no tiene BoxCollider");
            return false;
        }

        Vector3 localSize = boxCollider.size;

        // Calculamos las dimensiones en el espacio mundial considerando la rotación
        Vector3 worldX = player.TransformVector(new Vector3(localSize.x, 0, 0));
        Vector3 worldY = player.TransformVector(new Vector3(0, localSize.y, 0));
        Vector3 worldZ = player.TransformVector(new Vector3(0, 0, localSize.z));

        float sizeX = worldX.magnitude;
        float sizeY = worldY.magnitude;
        float sizeZ = worldZ.magnitude;

        // Está vertical si la altura (Y) es mayor que las dimensiones horizontales
        // Con escala (1,1,2), en vertical Y?2, y X y Z?1
        float height = sizeY;
        float maxHorizontal = Mathf.Max(sizeX, sizeZ);

        // Está vertical cuando la altura es la dimensión grande
        bool isVertical = height > maxHorizontal * 1.2f;

        Debug.Log($"Player orientation - Height: {height}, MaxHorizontal: {maxHorizontal}, IsVertical: {isVertical}");

        return isVertical;
    }

    IEnumerator LoadNextLevelWithDelay()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadScene(nextLevelName);
    }
}