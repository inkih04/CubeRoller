using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DivisionManager : MonoBehaviour
{
    public static DivisionManager Instance;

    [Header("Prefabs")]
    public GameObject halfCubePrefab;

    [Header("Referencias")]
    public MoveCube mainPlayer;
    public Material selectedMaterial;

    [Header("Configuración")]
    public float divisionDuration = 0.5f;
    public float mergeDistance = 1.1f;

    private Material[] originalMaterialsA;
    private Material[] originalMaterialsB;
    private GameObject halfA;
    private GameObject halfB;
    private MoveDividedCube scriptA;
    private MoveDividedCube scriptB;
    private bool isDivided = false;
    private int activeHalf = 0;
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

    public void ResetDivision()
    {
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
        originalMaterialsA = null;
        originalMaterialsB = null;

        if (mainPlayer == null)
        {
            mainPlayer = FindObjectOfType<MoveCube>();
        }
    }

    public void DividePlayer(Vector3 divisionPoint, bool isHorizontalSplit, float separationDistance = 0.5f)
    {
        if (isDivided || mainPlayer == null) return;

        StartCoroutine(DividePlayerCoroutine(divisionPoint, isHorizontalSplit, separationDistance));
    }

    public void DividePlayerAtPositions(Vector3 posA, Vector3 posB, Quaternion rotation)
    {
        if (isDivided) return;

        if (mainPlayer == null)
        {
            mainPlayer = FindObjectOfType<MoveCube>();
            if (mainPlayer == null) return;
        }

        StartCoroutine(DividePlayerAtPositionsCoroutine(posA, posB, rotation));
    }

    IEnumerator DividePlayerCoroutine(Vector3 divisionPoint, bool isHorizontalSplit, float separationDistance)
    {
        isDivided = true;

        mainPlayer.SetPlayerControl(false);
        mainPlayer.HidePlayer();

        Vector3 offsetDirection = isHorizontalSplit ? mainPlayer.transform.right : mainPlayer.transform.forward;
        Vector3 posA = divisionPoint + offsetDirection * separationDistance;
        Vector3 posB = divisionPoint - offsetDirection * separationDistance;

        yield return StartCoroutine(CreateHalves(posA, posB, mainPlayer.transform.rotation));
    }

    IEnumerator DividePlayerAtPositionsCoroutine(Vector3 posA, Vector3 posB, Quaternion rotation)
    {
        isDivided = true;

        mainPlayer.SetPlayerControl(false);
        mainPlayer.HidePlayer();

        yield return StartCoroutine(CreateHalves(posA, posB, rotation));
    }

    IEnumerator CreateHalves(Vector3 posA, Vector3 posB, Quaternion rotation)
    {
        halfA = Instantiate(halfCubePrefab, posA, rotation);
        halfB = Instantiate(halfCubePrefab, posB, rotation);

        scriptA = halfA.GetComponent<MoveDividedCube>();
        scriptB = halfB.GetComponent<MoveDividedCube>();

        if (scriptA == null || scriptB == null)
        {
            yield break;
        }

        scriptA.isActive = true;
        scriptB.isActive = false;
        scriptA.otherHalf = scriptB;
        scriptB.otherHalf = scriptA;

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
                }
            }
        }

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
        if (halfA == null || halfB == null || selectedMaterial == null) return;

        Renderer[] allRenderersA = halfA.GetComponentsInChildren<Renderer>();
        Renderer[] allRenderersB = halfB.GetComponentsInChildren<Renderer>();

        if (allRenderersA == null || allRenderersB == null) return;

        for (int i = 0; i < allRenderersA.Length; i++)
        {
            if (allRenderersA[i] != null)
            {
                if (scriptA.isActive)
                {
                    allRenderersA[i].material = selectedMaterial;
                }
                else
                {
                    if (originalMaterialsA != null && i < originalMaterialsA.Length && originalMaterialsA[i] != null)
                    {
                        allRenderersA[i].material = originalMaterialsA[i];
                    }
                }
            }
        }

        for (int i = 0; i < allRenderersB.Length; i++)
        {
            if (allRenderersB[i] != null)
            {
                if (scriptB.isActive)
                {
                    allRenderersB[i].material = selectedMaterial;
                }
                else
                {
                    if (originalMaterialsB != null && i < originalMaterialsB.Length && originalMaterialsB[i] != null)
                    {
                        allRenderersB[i].material = originalMaterialsB[i];
                    }
                }
            }
        }
    }

    void Update()
    {
        if (!isDivided) return;

        CheckMerge();
    }

    void CheckMerge()
    {
        if (halfA == null || halfB == null) return;
        if (scriptA.bMoving || scriptB.bMoving || scriptA.bFalling || scriptB.bFalling) return;

        float distance = Vector3.Distance(halfA.transform.position, halfB.transform.position);

        if (distance <= mergeDistance)
        {
            float yDiff = Mathf.Abs(halfA.transform.position.y - halfB.transform.position.y);

            if (yDiff < 0.2f)
            {
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
        midPoint.y = 1.20148f;

        Vector3 offset = halfA.transform.position - halfB.transform.position;
        offset.y = 0;

        Quaternion targetRotation;

        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.z))
        {
            targetRotation = Quaternion.Euler(0, 90, 0);
        }
        else
        {
            targetRotation = Quaternion.Euler(0, 0, 0);
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
        if (isDivided)
        {
            MergeHalves();
        }
    }
}