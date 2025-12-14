using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DivisionTile : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform cylinder;

    [Header("Spawn Points de las Mitades")]
    [SerializeField] private Transform spawnPointA;
    [SerializeField] private Transform spawnPointB;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private bool hasBeenPressed = false;
    private bool isCheckingInProgress = false;
    private float originalCylinderHeight;
    private Vector3 originalCylinderPosition;
    private BoxCollider boxCollider;
    private DivisionManager divisionManager;

    private void Awake()
    {
        // Suscribirse al evento de carga de escena para resetear el tile
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Guardar valores originales AQUÍ para que estén disponibles antes de cualquier reseteo
        if (cylinder != null)
        {
            originalCylinderHeight = cylinder.localScale.y;
            originalCylinderPosition = cylinder.localPosition;
        }

        boxCollider = GetComponent<BoxCollider>();

        if (showDebugLogs)
            Debug.Log($"[DivisionTile] {gameObject.name} Awake - valores originales guardados");
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Solo resetear si no es la primera carga (cuando los valores ya están guardados)
        if (originalCylinderHeight > 0)
        {
            if (showDebugLogs)
                Debug.Log($"<color=cyan>[DivisionTile] OnSceneLoaded - Reseteando tile: {gameObject.name} en escena {scene.name}</color>");

            ResetTile();

            // Asegurarse de que el DivisionManager también se resetee
            if (DivisionManager.Instance != null)
            {
                DivisionManager.Instance.ResetDivision();
            }
            else
            {
                Debug.LogWarning("[DivisionTile] No se encontró DivisionManager.Instance al cargar escena");
            }
        }
        else
        {
            if (showDebugLogs)
                Debug.Log($"[DivisionTile] OnSceneLoaded - Primera carga, saltando reseteo de {gameObject.name}");
        }
    }

    private void OnEnable()
    {
        // Resetear cuando el objeto se activa (por si acaso)
        if (showDebugLogs)
            Debug.Log($"[DivisionTile] OnEnable - {gameObject.name} activado. hasBeenPressed={hasBeenPressed}");
    }

    private void Start()
    {
        if (cylinder == null)
        {
            Debug.LogError($"[DivisionTile] El cilindro no ha sido asignado en el inspector para {gameObject.name}");
            return;
        }

        divisionManager = DivisionManager.Instance;

        if (divisionManager == null)
        {
            Debug.LogError($"[DivisionTile] No se encontró DivisionManager en la escena para {gameObject.name}");
        }

        if (spawnPointA == null || spawnPointB == null)
        {
            Debug.LogError($"[DivisionTile] IMPORTANTE: Debes asignar SpawnPointA y SpawnPointB en el inspector de {gameObject.name}");
        }

        if (showDebugLogs)
            Debug.Log($"<color=green>[DivisionTile] {gameObject.name} inicializado correctamente</color>");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (showDebugLogs)
            Debug.Log($"[DivisionTile] OnTriggerEnter - {gameObject.name} detectó colisión con {other.gameObject.name} (tag: {other.tag}). hasBeenPressed={hasBeenPressed}, isChecking={isCheckingInProgress}");

        if (other.CompareTag("Player") && !hasBeenPressed && !isCheckingInProgress)
        {
            if (showDebugLogs)
                Debug.Log($"<color=cyan>[DivisionTile] {gameObject.name} - Player ha entrado en trigger. Iniciando CheckVerticalAfterDelay...</color>");
            StartCoroutine(CheckVerticalAfterDelay(other.gameObject));
        }
        else
        {
            if (showDebugLogs)
                Debug.Log($"[DivisionTile] {gameObject.name} - Trigger ignorado (hasBeenPressed={hasBeenPressed}, isChecking={isCheckingInProgress})");
        }
    }

    IEnumerator CheckVerticalAfterDelay(GameObject player)
    {
        isCheckingInProgress = true;

        if (showDebugLogs)
            Debug.Log($"[DivisionTile] {gameObject.name} - Esperando 0.7s para verificar verticalidad...");

        // Esperar a que el jugador termine de moverse y se estabilice
        yield return new WaitForSeconds(0.7f);

        // Comprobar si el jugador está en posición vertical
        bool isVertical = MoveCube.Instance != null
            ? MoveCube.Instance.IsInVerticalPosition()
            : IsPlayerVertical(player);

        if (showDebugLogs)
            Debug.Log($"[DivisionTile] {gameObject.name} - ¿Vertical después de delay?: {isVertical}");

        if (isVertical)
        {
            if (showDebugLogs)
                Debug.Log($"<color=lime>[DivisionTile] {gameObject.name} - Tile activado correctamente - ¡Jugador en VERTICAL!</color>");
            hasBeenPressed = true;
            PressedTile(player);
        }
        else
        {
            if (showDebugLogs)
                Debug.Log($"<color=yellow>[DivisionTile] {gameObject.name} - No estaba en vertical, no se activa.</color>");
        }

        isCheckingInProgress = false;
    }

    private bool IsPlayerVertical(GameObject player)
    {
        return player.transform.up.y > 0.9f;
    }

    private void PressedTile(GameObject player)
    {
        if (showDebugLogs)
            Debug.Log($"<color=cyan>=== TILE PRESIONADO: {gameObject.name} (DivisionTile) ===</color>");

        // Deshabilitar el collider para que no se active de nuevo
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
            if (showDebugLogs)
                Debug.Log($"[DivisionTile] {gameObject.name} - Collider deshabilitado");
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

            if (showDebugLogs)
                Debug.Log($"[DivisionTile] {gameObject.name} - Cilindro bajado");
        }

        // Obtener SIEMPRE la instancia actual del DivisionManager (no usar la referencia guardada)
        DivisionManager currentDivisionManager = DivisionManager.Instance;

        if (showDebugLogs)
            Debug.Log($"[DivisionTile] {gameObject.name} - DivisionManager válido: {currentDivisionManager != null}, isDivided: {(currentDivisionManager != null ? currentDivisionManager.IsDivided().ToString() : "N/A")}");

        // Activar la división
        MoveCube playerScript = player.GetComponent<MoveCube>();

        if (showDebugLogs)
            Debug.Log($"[DivisionTile] {gameObject.name} - Verificando condiciones: DM={currentDivisionManager != null}, spawnA={spawnPointA != null}, spawnB={spawnPointB != null}, playerScript={playerScript != null}");

        if (currentDivisionManager != null && spawnPointA != null && spawnPointB != null && playerScript != null)
        {
            if (showDebugLogs)
                Debug.Log($"[DivisionTile] {gameObject.name} - Llamando a DividePlayerAtPositions con posA={spawnPointA.position}, posB={spawnPointB.position}");

            currentDivisionManager.DividePlayerAtPositions(
                spawnPointA.position,
                spawnPointB.position,
                playerScript.transform.rotation
            );

            if (showDebugLogs)
                Debug.Log($"<color=green>[DivisionTile] {gameObject.name} - DividePlayerAtPositions llamado!</color>");
        }
        else
        {
            Debug.LogError($"[DivisionTile] {gameObject.name} - No se puede dividir el jugador. divisionManager={currentDivisionManager != null}, spawnA={spawnPointA != null}, spawnB={spawnPointB != null}, playerScript={playerScript != null}");
        }
    }

    public void ResetTile()
    {
        if (showDebugLogs)
            Debug.Log($"<color=yellow>[DivisionTile] ResetTile llamado para {gameObject.name} - Estado antes: hasBeenPressed={hasBeenPressed}, isChecking={isCheckingInProgress}</color>");

        hasBeenPressed = false;
        isCheckingInProgress = false;

        // Restaurar el cilindro a su estado original
        if (cylinder != null)
        {
            cylinder.localScale = new Vector3(
                cylinder.localScale.x,
                originalCylinderHeight,
                cylinder.localScale.z
            );

            cylinder.localPosition = originalCylinderPosition;

            if (showDebugLogs)
                Debug.Log($"[DivisionTile] {gameObject.name} - Cilindro restaurado a: pos={originalCylinderPosition}, altura={originalCylinderHeight}");
        }

        // Reactivar el collider
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }

        if (showDebugLogs)
            Debug.Log($"<color=green>[DivisionTile] {gameObject.name} reseteado completamente - hasBeenPressed={hasBeenPressed}, collider enabled={boxCollider != null && boxCollider.enabled}</color>");
    }

    // Método estático para resetear todos los tiles de división en la escena
    public static void ResetAllDivisionTiles()
    {
        DivisionTile[] allTiles = FindObjectsOfType<DivisionTile>();
        Debug.Log($"<color=yellow>[DivisionTile] ResetAllDivisionTiles - Encontrados {allTiles.Length} tiles</color>");

        foreach (DivisionTile tile in allTiles)
        {
            if (tile != null)
            {
                Debug.Log($"[DivisionTile] Reseteando tile: {tile.gameObject.name}");
                tile.ResetTile();
            }
        }

        Debug.Log($"<color=green>[DivisionTile] {allTiles.Length} tiles reseteados</color>");
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