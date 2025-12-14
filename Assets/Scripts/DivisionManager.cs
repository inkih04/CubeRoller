using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DivisionManager : MonoBehaviour
{
    public static DivisionManager Instance;

    [Header("Prefabs")]
    public GameObject halfCubePrefab; // Prefab de la mitad del cubo

    [Header("Referencias")]
    public MoveCube mainPlayer;
    public Material normalMaterial;
    public Material selectedMaterial; // Material con brillo verde

    [Header("Configuración")]
    public float divisionDuration = 0.5f;
    public float mergeDistance = 1.1f; // Distancia para considerar juntar las mitades

    [Header("Debug")]
    public bool showDebugLogs = true;

    private GameObject halfA;
    private GameObject halfB;
    private MoveDividedCube scriptA;
    private MoveDividedCube scriptB;

    private bool isDivided = false;
    private int activeHalf = 0; // 0 = halfA, 1 = halfB

    private InputAction switchAction;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ResetDivision()
    {
        if (showDebugLogs)
            Debug.Log($"<color=yellow>[DivisionManager] ResetDivision llamado. Antes - isDivided={isDivided}, halfA={halfA != null}, halfB={halfB != null}</color>");

        // Limpiar las mitades si existen
        if (halfA != null)
        {
            Destroy(halfA);
            halfA = null;
        }

        if (halfB != null)
        {
            Destroy(halfB);
            halfB = null;
        }

        scriptA = null;
        scriptB = null;
        isDivided = false;
        activeHalf = 0;

        // Asegurarse de que mainPlayer esté disponible
        if (mainPlayer == null)
        {
            mainPlayer = FindObjectOfType<MoveCube>();
            if (mainPlayer != null)
            {
                if (showDebugLogs)
                    Debug.Log("<color=green>[DivisionManager] mainPlayer re-asignado automáticamente</color>");
            }
            else
            {
                Debug.LogWarning("[DivisionManager] No se pudo encontrar MoveCube en la escena");
            }
        }

        if (showDebugLogs)
            Debug.Log($"<color=green>[DivisionManager] Estado reseteado. Después - isDivided={isDivided}, mainPlayer válido={mainPlayer != null}</color>");
    }

    void Start()
    {
        // Configurar la acción de cambio (tecla Espacio)
        switchAction = new InputAction("Switch", binding: "<Keyboard>/space");
        switchAction.performed += ctx => SwitchActiveHalf();
        switchAction.Enable();

        if (showDebugLogs)
            Debug.Log("[DivisionManager] Inicializado correctamente");
    }

    void OnDestroy()
    {
        if (switchAction != null)
        {
            switchAction.Disable();
            switchAction.Dispose();
        }
    }

    public void DividePlayer(Vector3 divisionPoint, bool isHorizontalSplit, float separationDistance = 0.5f)
    {
        if (isDivided)
        {
            Debug.LogWarning("[DivisionManager] No se puede dividir: ya está dividido");
            return;
        }

        if (mainPlayer == null)
        {
            Debug.LogError("[DivisionManager] No se puede dividir: mainPlayer es null");
            return;
        }

        if (showDebugLogs)
            Debug.Log($"<color=cyan>[DivisionManager] Dividiendo jugador en punto {divisionPoint}</color>");

        StartCoroutine(DividePlayerCoroutine(divisionPoint, isHorizontalSplit, separationDistance));
    }

    public void DividePlayerAtPositions(Vector3 posA, Vector3 posB, Quaternion rotation)
    {
        if (showDebugLogs)
            Debug.Log($"<color=magenta>[DivisionManager] ===== DividePlayerAtPositions LLAMADO ===== posA={posA}, posB={posB}</color>");

        if (isDivided)
        {
            Debug.LogWarning($"<color=red>[DivisionManager] ? NO SE PUEDE DIVIDIR: Ya está dividido (isDivided={isDivided})</color>");
            return;
        }

        if (showDebugLogs)
            Debug.Log($"[DivisionManager] ? isDivided check passed (isDivided={isDivided})");

        if (mainPlayer == null)
        {
            Debug.LogError("<color=red>[DivisionManager] ? NO SE PUEDE DIVIDIR: mainPlayer es null - intentando encontrarlo...</color>");
            mainPlayer = FindObjectOfType<MoveCube>();
            if (mainPlayer == null)
            {
                Debug.LogError("<color=red>[DivisionManager] ? No se pudo encontrar MoveCube en la escena!</color>");
                return;
            }
            Debug.Log("<color=green>[DivisionManager] ? MoveCube encontrado y asignado!</color>");
        }

        if (showDebugLogs)
            Debug.Log($"<color=cyan>[DivisionManager] ? Todas las verificaciones pasadas. Iniciando coroutine...</color>");

        StartCoroutine(DividePlayerAtPositionsCoroutine(posA, posB, rotation));

        if (showDebugLogs)
            Debug.Log($"<color=green>[DivisionManager] ? Coroutine iniciada exitosamente</color>");
    }

    IEnumerator DividePlayerCoroutine(Vector3 divisionPoint, bool isHorizontalSplit, float separationDistance)
    {
        isDivided = true;

        if (showDebugLogs)
            Debug.Log("[DivisionManager] Coroutine iniciada - desactivando jugador principal");

        // Desactivar el jugador principal
        mainPlayer.SetPlayerControl(false);
        mainPlayer.HidePlayer();

        // Determinar posiciones de las mitades según la orientación del jugador
        Vector3 offsetDirection = isHorizontalSplit ?
            mainPlayer.transform.right : mainPlayer.transform.forward;

        Vector3 posA = divisionPoint + offsetDirection * separationDistance;
        Vector3 posB = divisionPoint - offsetDirection * separationDistance;

        yield return StartCoroutine(CreateHalves(posA, posB, mainPlayer.transform.rotation));
    }

    IEnumerator DividePlayerAtPositionsCoroutine(Vector3 posA, Vector3 posB, Quaternion rotation)
    {
        isDivided = true;

        if (showDebugLogs)
            Debug.Log("[DivisionManager] Coroutine iniciada (posiciones específicas) - desactivando jugador principal");

        // Desactivar el jugador principal
        mainPlayer.SetPlayerControl(false);
        mainPlayer.HidePlayer();

        yield return StartCoroutine(CreateHalves(posA, posB, rotation));
    }


    IEnumerator CreateHalves(Vector3 posA, Vector3 posB, Quaternion rotation)
    {
        if (showDebugLogs)
            Debug.Log($"[DivisionManager] Creando mitades en posiciones: A={posA}, B={posB}");

        // Crear las mitades
        halfA = Instantiate(halfCubePrefab, posA, rotation);
        halfB = Instantiate(halfCubePrefab, posB, rotation);

        if (showDebugLogs)
            Debug.Log($"[DivisionManager] Mitades instanciadas: halfA={halfA != null}, halfB={halfB != null}");

        scriptA = halfA.GetComponent<MoveDividedCube>();
        scriptB = halfB.GetComponent<MoveDividedCube>();

        if (scriptA == null || scriptB == null)
        {
            Debug.LogError($"[DivisionManager] Los prefabs de mitad necesitan el componente MoveDividedCube - scriptA={scriptA != null}, scriptB={scriptB != null}");
            yield break;
        }

        // Configurar los scripts
        scriptA.isActive = true;
        scriptB.isActive = false;
        scriptA.otherHalf = scriptB;
        scriptB.otherHalf = scriptA;

        if (showDebugLogs)
            Debug.Log("[DivisionManager] Scripts configurados correctamente");

        // Aplicar materiales
        UpdateVisuals();

        yield return new WaitForSeconds(0.1f);

        activeHalf = 0;

        if (showDebugLogs)
            Debug.Log("<color=green>[DivisionManager] División completada exitosamente!</color>");
    }

    void SwitchActiveHalf()
    {
        if (!isDivided) return;

        activeHalf = 1 - activeHalf;

        scriptA.isActive = (activeHalf == 0);
        scriptB.isActive = (activeHalf == 1);

        UpdateVisuals();

        if (showDebugLogs)
            Debug.Log($"[DivisionManager] Cambiado a mitad {(activeHalf == 0 ? "A" : "B")}");
    }

    void UpdateVisuals()
    {
        if (halfA != null && halfB != null && selectedMaterial != null && normalMaterial != null)
        {
            Renderer rendA = halfA.GetComponent<Renderer>();
            Renderer rendB = halfB.GetComponent<Renderer>();

            if (rendA != null)
                rendA.material = scriptA.isActive ? selectedMaterial : normalMaterial;

            if (rendB != null)
                rendB.material = scriptB.isActive ? selectedMaterial : normalMaterial;
        }
    }

    void Update()
    {
        if (!isDivided) return;

        // Comprobar si las mitades están lo suficientemente cerca para unirse
        CheckMerge();
    }

    void CheckMerge()
    {
        if (halfA == null || halfB == null) return;
        if (scriptA.bMoving || scriptB.bMoving || scriptA.bFalling || scriptB.bFalling) return;

        float distance = Vector3.Distance(halfA.transform.position, halfB.transform.position);

        if (distance <= mergeDistance)
        {
            // Verificar que estén alineados (misma Y aproximadamente)
            float yDiff = Mathf.Abs(halfA.transform.position.y - halfB.transform.position.y);

            if (yDiff < 0.2f)
            {
                // Verificar que estén adyacentes (no uno encima del otro)
                Vector3 offset = halfA.transform.position - halfB.transform.position;
                float xzDistance = new Vector2(offset.x, offset.z).magnitude;

                if (xzDistance > 0.5f && xzDistance < mergeDistance)
                {
                    if (showDebugLogs)
                        Debug.Log("[DivisionManager] Condiciones de merge cumplidas - iniciando unión");
                    MergeHalves();
                }
            }
        }
    }

    void MergeHalves()
    {
        if (!isDivided) return;

        Vector3 midPoint = (halfA.transform.position + halfB.transform.position) / 2f;
        midPoint.y = 1.20148f;

        // Calcular la orientación basada en la posición de las mitades
        Vector3 offset = halfA.transform.position - halfB.transform.position;
        offset.y = 0; // Ignorar diferencia en Y

        Quaternion targetRotation;

        // Determinar si están alineados en X o Z
        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.z))
        {
            // Alineados en el eje X
            targetRotation = Quaternion.Euler(0, 90, 0);
            if (showDebugLogs)
                Debug.Log("[DivisionManager] Mitades alineadas en eje X - rotación 90°");
        }
        else
        {
            // Alineados en el eje Z
            targetRotation = Quaternion.Euler(0, 0, 0);
            if (showDebugLogs)
                Debug.Log("[DivisionManager] Mitades alineadas en eje Z - rotación 0°");
        }

        mainPlayer.transform.position = midPoint;
        mainPlayer.transform.rotation = targetRotation;

        Renderer[] renderers = mainPlayer.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
            r.enabled = true;

        mainPlayer.SetPlayerControl(true);

        Destroy(halfA);
        Destroy(halfB);

        isDivided = false;

        if (showDebugLogs)
            Debug.Log($"<color=green>[DivisionManager] ¡Mitades unidas! Posición: {midPoint}, Rotación: {targetRotation.eulerAngles}</color>");
    }


    public bool IsDivided()
    {
        return isDivided;
    }

    public int GetActiveHalf()
    {
        return activeHalf;
    }

    public GameObject GetHalfA()
    {
        return halfA;
    }

    public GameObject GetHalfB()
    {
        return halfB;
    }

    public void ForceMerge()
    {
        // Método útil para debug o mecánicas especiales
        if (isDivided)
        {
            if (showDebugLogs)
                Debug.Log("[DivisionManager] ForceMerge llamado");
            MergeHalves();
        }
    }
}