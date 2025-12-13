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

    public void DividePlayer(Vector3 divisionPoint, bool isHorizontalSplit)
    {
        if (isDivided || mainPlayer == null) return;

        StartCoroutine(DividePlayerCoroutine(divisionPoint, isHorizontalSplit));
    }

    IEnumerator DividePlayerCoroutine(Vector3 divisionPoint, bool isHorizontalSplit)
    {
        isDivided = true;

        // Desactivar el jugador principal
        mainPlayer.SetPlayerControl(false);
        mainPlayer.HidePlayer();

        // Determinar posiciones de las mitades según la orientación del jugador
        Vector3 offsetDirection = isHorizontalSplit ?
            mainPlayer.transform.right : mainPlayer.transform.forward;

        Vector3 posA = divisionPoint + offsetDirection * 0.5f;
        Vector3 posB = divisionPoint - offsetDirection * 0.5f;

        // Crear las mitades
        halfA = Instantiate(halfCubePrefab, posA, mainPlayer.transform.rotation);
        halfB = Instantiate(halfCubePrefab, posB, mainPlayer.transform.rotation);

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

        // Calcular punto medio
        Vector3 midPoint = (halfA.transform.position + halfB.transform.position) / 2f;

        // Determinar rotación (usar la de la mitad activa)
        Quaternion rotation = activeHalf == 0 ?
            halfA.transform.rotation : halfB.transform.rotation;

        // Reactivar el jugador principal
        mainPlayer.transform.position = midPoint;
        mainPlayer.transform.rotation = rotation;

        // Hacer visible el jugador principal
        Renderer[] renderers = mainPlayer.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
            r.enabled = true;

        mainPlayer.SetPlayerControl(true);

        // Destruir las mitades
        Destroy(halfA);
        Destroy(halfB);

        isDivided = false;

        Debug.Log("¡Mitades unidas!");
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