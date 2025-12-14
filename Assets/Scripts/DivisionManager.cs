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

        Debug.Log("[DivisionManager] Estado reseteado");
    }

    void Start()
    {
        // Configurar la acción de cambio (tecla Espacio)
        switchAction = new InputAction("Switch", binding: "<Keyboard>/space");
        switchAction.performed += ctx => SwitchActiveHalf();
        switchAction.Enable();
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
        if (isDivided || mainPlayer == null) return;

        StartCoroutine(DividePlayerCoroutine(divisionPoint, isHorizontalSplit, separationDistance));
    }

    public void DividePlayerAtPositions(Vector3 posA, Vector3 posB, Quaternion rotation)
    {
        if (isDivided || mainPlayer == null) return;

        StartCoroutine(DividePlayerAtPositionsCoroutine(posA, posB, rotation));
    }

    IEnumerator DividePlayerCoroutine(Vector3 divisionPoint, bool isHorizontalSplit, float separationDistance)
    {
        isDivided = true;

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

        // Desactivar el jugador principal
        mainPlayer.SetPlayerControl(false);
        mainPlayer.HidePlayer();

        yield return StartCoroutine(CreateHalves(posA, posB, rotation));
    }


    IEnumerator CreateHalves(Vector3 posA, Vector3 posB, Quaternion rotation)
    {
        // Crear las mitades
        halfA = Instantiate(halfCubePrefab, posA, rotation);
        halfB = Instantiate(halfCubePrefab, posB, rotation);

        scriptA = halfA.GetComponent<MoveDividedCube>();
        scriptB = halfB.GetComponent<MoveDividedCube>();

        if (scriptA == null || scriptB == null)
        {
            Debug.LogError("Los prefabs de mitad necesitan el componente MoveDividedCube");
            yield break;
        }

        // Configurar los scripts
        scriptA.isActive = true;
        scriptB.isActive = false;
        scriptA.otherHalf = scriptB;
        scriptB.otherHalf = scriptA;

        // Aplicar materiales
        UpdateVisuals();

        yield return new WaitForSeconds(0.1f);

        activeHalf = 0;
    }

    void SwitchActiveHalf()
    {
        if (!isDivided) return;

        activeHalf = 1 - activeHalf;

        scriptA.isActive = (activeHalf == 0);
        scriptB.isActive = (activeHalf == 1);

        UpdateVisuals();
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
                    MergeHalves();
                }
            }
        }
    }

    void MergeHalves()
    {
        if (!isDivided) return;

        Vector3 midPoint = (halfA.transform.position + halfB.transform.position) / 2f;
        midPoint.y += 0.5f;

        // Calcular la orientación basada en la posición de las mitades
        Vector3 offset = halfA.transform.position - halfB.transform.position;
        offset.y = 0; // Ignorar diferencia en Y

        Quaternion targetRotation;

        // Determinar si están alineados en X o Z
        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.z))
        {
            // Alineados en el eje X
            targetRotation = Quaternion.Euler(0, 90, 0);
            Debug.Log("Mitades alineadas en eje X - rotación 90°");
        }
        else
        {
            // Alineados en el eje Z
            targetRotation = Quaternion.Euler(0, 0, 0);
            Debug.Log("Mitades alineadas en eje Z - rotación 0°");
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

        Debug.Log($"¡Mitades unidas! Posición: {midPoint}, Rotación: {targetRotation.eulerAngles}");
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
            MergeHalves();
        }
    }
}