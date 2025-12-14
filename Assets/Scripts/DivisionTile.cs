using System.Collections;
using UnityEngine;

public class DivisionTile : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform cylinder;

    [Header("Spawn Points de las Mitades")]
    [SerializeField] private Transform spawnPointA;
    [SerializeField] private Transform spawnPointB;

    private bool hasBeenPressed = false;
    private bool isCheckingInProgress = false;
    private float originalCylinderHeight;
    private BoxCollider boxCollider;
    private DivisionManager divisionManager;

    private void Start()
    {
        if (cylinder == null)
        {
            Debug.LogError("El cilindro no ha sido asignado en el inspector para " + gameObject.name);
            return;
        }

        originalCylinderHeight = cylinder.localScale.y;
        boxCollider = GetComponent<BoxCollider>();
        divisionManager = DivisionManager.Instance;

        if (divisionManager == null)
        {
            Debug.LogError("No se encontró DivisionManager en la escena");
        }

        if (spawnPointA == null || spawnPointB == null)
        {
            Debug.LogError("IMPORTANTE: Debes asignar SpawnPointA y SpawnPointB en el inspector de " + gameObject.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenPressed && !isCheckingInProgress)
        {
            Debug.Log("[DivisionTile] Player ha entrado en trigger.");
            StartCoroutine(CheckVerticalAfterDelay(other.gameObject));
        }
    }

    IEnumerator CheckVerticalAfterDelay(GameObject player)
    {
        isCheckingInProgress = true;

        // Esperar a que el jugador termine de moverse y se estabilice
        yield return new WaitForSeconds(0.7f);

        // Comprobar si el jugador está en posición vertical
        bool isVertical = MoveCube.Instance != null
            ? MoveCube.Instance.IsInVerticalPosition()
            : IsPlayerVertical(player);

        Debug.Log($"[DivisionTile] ¿Vertical después de delay?: {isVertical}");

        if (isVertical)
        {
            Debug.Log("<color=cyan>[DivisionTile] Tile activado correctamente - ¡Jugador en VERTICAL!</color>");
            hasBeenPressed = true;
            PressedTile(player);
        }
        else
        {
            Debug.Log("<color=yellow>[DivisionTile] No estaba en vertical, no se activa.</color>");
        }

        isCheckingInProgress = false;
    }

    private bool IsPlayerVertical(GameObject player)
    {
        return player.transform.up.y > 0.9f;
    }

    private void PressedTile(GameObject player)
    {
        Debug.Log("=== TILE PRESIONADO (DivisionTile) ===");

        // Deshabilitar el collider para que no se active de nuevo
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        // Bajar el cilindro
        if (cylinder != null)
        {
            Vector3 newScale = cylinder.localScale;
            newScale.y -= 0.1f;
            cylinder.localScale = newScale;

            cylinder.localPosition = new Vector3(
                cylinder.localPosition.x,
                cylinder.localPosition.y - 0.05f,
                cylinder.localPosition.z
            );
        }

        // Activar la división
        MoveCube playerScript = player.GetComponent<MoveCube>();
        if (divisionManager != null && spawnPointA != null && spawnPointB != null && playerScript != null)
        {
            divisionManager.DividePlayerAtPositions(
                spawnPointA.position,
                spawnPointB.position,
                playerScript.transform.rotation
            );

            Debug.Log("Tile presionado: " + gameObject.name + " - ¡Jugador dividido!");
        }
        else
        {
            Debug.LogError("No se puede dividir el jugador. Verifica que SpawnPointA y SpawnPointB estén asignados.");
        }
    }

    public void ResetTile()
    {
        hasBeenPressed = false;
        isCheckingInProgress = false;

        // Restaurar el cilindro
        if (cylinder != null)
        {
            cylinder.localScale = new Vector3(
                cylinder.localScale.x,
                originalCylinderHeight,
                cylinder.localScale.z
            );

            cylinder.localPosition = new Vector3(
                cylinder.localPosition.x,
                cylinder.localPosition.y + 0.05f,
                cylinder.localPosition.z
            );
        }

        // Reactivar el collider
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
    }

    private void OnDrawGizmos()
    {
        // Dibujar spawn points si están asignados
        if (spawnPointA != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPointA.position, 0.25f);
            Gizmos.DrawLine(transform.position, spawnPointA.position);

#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.Label(spawnPointA.position + Vector3.up * 0.4f, "MITAD A", new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.green },
                fontStyle = FontStyle.Bold,
                fontSize = 12
            });
#endif
        }

        if (spawnPointB != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(spawnPointB.position, 0.25f);
            Gizmos.DrawLine(transform.position, spawnPointB.position);

#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.magenta;
            UnityEditor.Handles.Label(spawnPointB.position + Vector3.up * 0.4f, "MITAD B", new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.magenta },
                fontStyle = FontStyle.Bold,
                fontSize = 12
            });
#endif
        }

        // Dibujar línea entre spawn points
        if (spawnPointA != null && spawnPointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(spawnPointA.position, spawnPointB.position);
        }

        // Dibujar el centro del tile
        Gizmos.color = hasBeenPressed ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.1f, new Vector3(1f, 0.2f, 1f));
    }
}