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
    public Material selectedMaterial; // Material con brillo verde (OBLIGATORIO)

    [Header("Materiales Originales (Auto)")]
    private Material[] originalMaterialsA; // Array de materiales originales de A
    private Material[] originalMaterialsB; // Array de materiales originales de B

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
        
        // Limpiar arrays de materiales guardados
        originalMaterialsA = null;
        originalMaterialsB = null;

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

        // VERIFICAR MATERIALES AL INICIO
        if (showDebugLogs)
        {
            Debug.Log("<color=cyan>===== VERIFICACIÓN DE MATERIALES AL INICIO =====</color>");
            
            Debug.Log($"[DivisionManager] selectedMaterial asignado: {selectedMaterial != null}");
            if (selectedMaterial != null)
                Debug.Log($"[DivisionManager] selectedMaterial nombre: {selectedMaterial.name}");
            else
                Debug.LogError("<color=red>[DivisionManager] ¡FALTA ASIGNAR selectedMaterial (verde) EN EL INSPECTOR!</color>");
            
            Debug.Log("<color=cyan>================================================</color>");
        }

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

        // GUARDAR LOS MATERIALES ORIGINALES DE TODOS LOS RENDERERS
        Renderer[] renderersA = halfA.GetComponentsInChildren<Renderer>();
        Renderer[] renderersB = halfB.GetComponentsInChildren<Renderer>();

        if (renderersA != null && renderersA.Length > 0)
        {
            originalMaterialsA = new Material[renderersA.Length];
            for (int i = 0; i < renderersA.Length; i++)
            {
                if (renderersA[i] != null && renderersA[i].material != null)
                {
                    originalMaterialsA[i] = renderersA[i].material;
                    if (showDebugLogs)
                        Debug.Log($"<color=lime>[DivisionManager] ? Material original {i} de Mitad A guardado: {originalMaterialsA[i].name} (de {renderersA[i].gameObject.name})</color>");
                }
            }
        }

        if (renderersB != null && renderersB.Length > 0)
        {
            originalMaterialsB = new Material[renderersB.Length];
            for (int i = 0; i < renderersB.Length; i++)
            {
                if (renderersB[i] != null && renderersB[i].material != null)
                {
                    originalMaterialsB[i] = renderersB[i].material;
                    if (showDebugLogs)
                        Debug.Log($"<color=lime>[DivisionManager] ? Material original {i} de Mitad B guardado: {originalMaterialsB[i].name} (de {renderersB[i].gameObject.name})</color>");
                }
            }
        }

        // Aplicar materiales
        if (showDebugLogs)
            Debug.Log("<color=yellow>[DivisionManager] Llamando a UpdateVisuals() por primera vez...</color>");
        
        UpdateVisuals();

        yield return new WaitForSeconds(0.1f);

        activeHalf = 0;

        if (showDebugLogs)
            Debug.Log("<color=green>[DivisionManager] División completada exitosamente!</color>");
    }

    void SwitchActiveHalf()
    {
        if (!isDivided)
        {
            if (showDebugLogs)
                Debug.Log("<color=yellow>[DivisionManager] SwitchActiveHalf ignorado - no está dividido</color>");
            return;
        }

        if (showDebugLogs)
            Debug.Log($"<color=cyan>===== CAMBIANDO MITAD ACTIVA =====</color>");
        
        int oldActiveHalf = activeHalf;
        activeHalf = 1 - activeHalf;

        if (showDebugLogs)
            Debug.Log($"[DivisionManager] activeHalf cambió de {oldActiveHalf} a {activeHalf}");

        scriptA.isActive = (activeHalf == 0);
        scriptB.isActive = (activeHalf == 1);

        if (showDebugLogs)
        {
            Debug.Log($"[DivisionManager] scriptA.isActive = {scriptA.isActive}");
            Debug.Log($"[DivisionManager] scriptB.isActive = {scriptB.isActive}");
        }

        UpdateVisuals();

        if (showDebugLogs)
            Debug.Log($"<color=green>[DivisionManager] ? Cambiado a mitad {(activeHalf == 0 ? "A" : "B")}</color>");
    }

    void UpdateVisuals()
    {
        if (showDebugLogs)
            Debug.Log("<color=yellow>========== UPDATE VISUALS LLAMADO ==========</color>");

        // VERIFICACIONES PASO A PASO
        if (halfA == null)
        {
            Debug.LogError("<color=red>[UpdateVisuals] ? halfA es NULL!</color>");
            return;
        }
        if (showDebugLogs)
            Debug.Log($"[UpdateVisuals] ? halfA válido: {halfA.name}");

        if (halfB == null)
        {
            Debug.LogError("<color=red>[UpdateVisuals] ? halfB es NULL!</color>");
            return;
        }
        if (showDebugLogs)
            Debug.Log($"[UpdateVisuals] ? halfB válido: {halfB.name}");

        if (selectedMaterial == null)
        {
            Debug.LogError("<color=red>[UpdateVisuals] ? selectedMaterial es NULL! Asigna el material verde en el Inspector</color>");
            return;
        }
        if (showDebugLogs)
        {
            Debug.Log($"[UpdateVisuals] ? selectedMaterial válido: {selectedMaterial.name}");
            
            // INSPECCIONAR PROPIEDADES DEL MATERIAL VERDE
            Debug.Log($"<color=lime>===== INSPECCIONANDO MATERIAL VERDE =====</color>");
            Debug.Log($"[UpdateVisuals] Shader: {selectedMaterial.shader.name}");
            
            if (selectedMaterial.HasProperty("_Color"))
            {
                Color color = selectedMaterial.GetColor("_Color");
                Debug.Log($"[UpdateVisuals] Color principal: R={color.r:F2}, G={color.g:F2}, B={color.b:F2}, A={color.a:F2}");
            }
            
            if (selectedMaterial.HasProperty("_EmissionColor"))
            {
                Color emissionColor = selectedMaterial.GetColor("_EmissionColor");
                Debug.Log($"[UpdateVisuals] Color emisión: R={emissionColor.r:F2}, G={emissionColor.g:F2}, B={emissionColor.b:F2}, intensidad={emissionColor.maxColorComponent:F2}");
                
                if (emissionColor.maxColorComponent > 0.01f)
                    Debug.Log($"<color=lime>[UpdateVisuals] ? Material TIENE emisión activa</color>");
                else
                    Debug.LogWarning($"<color=orange>[UpdateVisuals] ? Material NO tiene emisión activa (muy baja o 0)</color>");
            }
            else
            {
                Debug.LogWarning($"<color=orange>[UpdateVisuals] ? Material no tiene propiedad _EmissionColor</color>");
            }
            
            Debug.Log($"<color=lime>==========================================</color>");
        }

        // Verificar materiales originales
        if (originalMaterialsA == null || originalMaterialsA.Length == 0)
        {
            Debug.LogWarning("<color=orange>[UpdateVisuals] ? originalMaterialsA es NULL o vacío!</color>");
        }
        else if (showDebugLogs)
        {
            Debug.Log($"[UpdateVisuals] ? originalMaterialsA tiene {originalMaterialsA.Length} materiales guardados");
            
            // COMPARAR PRIMER MATERIAL ORIGINAL CON MATERIAL VERDE
            if (originalMaterialsA.Length > 0 && originalMaterialsA[0] != null)
            {
                Debug.Log($"<color=cyan>===== COMPARANDO MATERIALES =====</color>");
                
                if (originalMaterialsA[0].HasProperty("_Color") && selectedMaterial.HasProperty("_Color"))
                {
                    Color colorOriginal = originalMaterialsA[0].GetColor("_Color");
                    Color colorVerde = selectedMaterial.GetColor("_Color");
                    
                    Debug.Log($"[Comparación] Color ORIGINAL: R={colorOriginal.r:F2}, G={colorOriginal.g:F2}, B={colorOriginal.b:F2}");
                    Debug.Log($"[Comparación] Color VERDE: R={colorVerde.r:F2}, G={colorVerde.g:F2}, B={colorVerde.b:F2}");
                    
                    if (Vector3.Distance(new Vector3(colorOriginal.r, colorOriginal.g, colorOriginal.b), 
                                         new Vector3(colorVerde.r, colorVerde.g, colorVerde.b)) < 0.1f)
                    {
                        Debug.LogWarning("<color=red>[Comparación] ??? LOS COLORES SON MUY SIMILARES! Por eso no se nota la diferencia visual ???</color>");
                    }
                    else
                    {
                        Debug.Log("<color=lime>[Comparación] ? Los colores son diferentes</color>");
                    }
                }
                
                Debug.Log($"<color=cyan>=================================</color>");
            }
        }
        
        if (originalMaterialsB == null || originalMaterialsB.Length == 0)
        {
            Debug.LogWarning("<color=orange>[UpdateVisuals] ? originalMaterialsB es NULL o vacío!</color>");
        }

        // VERIFICAR SI HAY MÚLTIPLES RENDERERS
        Renderer[] allRenderersA = halfA.GetComponentsInChildren<Renderer>();
        Renderer[] allRenderersB = halfB.GetComponentsInChildren<Renderer>();
        
        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[UpdateVisuals] Mitad A tiene {allRenderersA.Length} renderer(s)</color>");
            for (int i = 0; i < allRenderersA.Length; i++)
            {
                Debug.Log($"[UpdateVisuals] - Renderer A[{i}]: {allRenderersA[i].gameObject.name} (Material actual: {allRenderersA[i].material.name})");
            }
            
            Debug.Log($"<color=cyan>[UpdateVisuals] Mitad B tiene {allRenderersB.Length} renderer(s)</color>");
            for (int i = 0; i < allRenderersB.Length; i++)
            {
                Debug.Log($"[UpdateVisuals] - Renderer B[{i}]: {allRenderersB[i].gameObject.name} (Material actual: {allRenderersB[i].material.name})");
            }
        }

        if (allRenderersA == null || allRenderersA.Length == 0)
        {
            Debug.LogError("<color=red>[UpdateVisuals] ? No se encontraron Renderers en halfA!</color>");
            return;
        }

        if (allRenderersB == null || allRenderersB.Length == 0)
        {
            Debug.LogError("<color=red>[UpdateVisuals] ? No se encontraron Renderers en halfB!</color>");
            return;
        }

        if (showDebugLogs)
        {
            Debug.Log($"[UpdateVisuals] ? Estado actual - scriptA.isActive: {scriptA.isActive}, scriptB.isActive: {scriptB.isActive}");
        }

        // APLICAR MATERIALES A TODOS LOS RENDERERS
        if (allRenderersA != null && allRenderersA.Length > 0)
        {
            if (showDebugLogs)
                Debug.Log($"<color=yellow>[UpdateVisuals] Aplicando materiales a MITAD A ({(scriptA.isActive ? "ACTIVA - verde" : "INACTIVA - originales")})</color>");
            
            for (int i = 0; i < allRenderersA.Length; i++)
            {
                if (allRenderersA[i] != null)
                {
                    Material materialToApply;
                    
                    if (scriptA.isActive)
                    {
                        // Si está activa, usar el material verde
                        materialToApply = selectedMaterial;
                    }
                    else
                    {
                        // Si está inactiva, usar el material original de ESTE renderer específico
                        if (originalMaterialsA != null && i < originalMaterialsA.Length && originalMaterialsA[i] != null)
                        {
                            materialToApply = originalMaterialsA[i];
                        }
                        else
                        {
                            Debug.LogWarning($"<color=orange>[UpdateVisuals] ? No hay material original guardado para renderer A[{i}], saltando...</color>");
                            continue;
                        }
                    }
                    
                    allRenderersA[i].material = materialToApply;
                    
                    if (showDebugLogs)
                        Debug.Log($"[UpdateVisuals] ? Material aplicado a A[{i}] ({allRenderersA[i].gameObject.name}): {materialToApply.name}");
                }
            }
        }

        if (allRenderersB != null && allRenderersB.Length > 0)
        {
            if (showDebugLogs)
                Debug.Log($"<color=yellow>[UpdateVisuals] Aplicando materiales a MITAD B ({(scriptB.isActive ? "ACTIVA - verde" : "INACTIVA - originales")})</color>");
            
            for (int i = 0; i < allRenderersB.Length; i++)
            {
                if (allRenderersB[i] != null)
                {
                    Material materialToApply;
                    
                    if (scriptB.isActive)
                    {
                        // Si está activa, usar el material verde
                        materialToApply = selectedMaterial;
                    }
                    else
                    {
                        // Si está inactiva, usar el material original de ESTE renderer específico
                        if (originalMaterialsB != null && i < originalMaterialsB.Length && originalMaterialsB[i] != null)
                        {
                            materialToApply = originalMaterialsB[i];
                        }
                        else
                        {
                            Debug.LogWarning($"<color=orange>[UpdateVisuals] ? No hay material original guardado para renderer B[{i}], saltando...</color>");
                            continue;
                        }
                    }
                    
                    allRenderersB[i].material = materialToApply;
                    
                    if (showDebugLogs)
                        Debug.Log($"[UpdateVisuals] ? Material aplicado a B[{i}] ({allRenderersB[i].gameObject.name}): {materialToApply.name}");
                }
            }
        }

        if (showDebugLogs)
            Debug.Log("<color=green>[UpdateVisuals] ========== UPDATE VISUALS COMPLETADO ==========</color>");
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