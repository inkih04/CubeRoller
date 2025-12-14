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

    [Header("Audio")]
    [SerializeField] private AudioClip buttonSound;
    [SerializeField][Range(0f, 1f)] private float soundVolume = 1f;

    private bool isCheckingInProgress = false;
    private float originalCylinderHeight;
    private Vector3 originalCylinderPosition;
    private BoxCollider boxCollider;
    private DivisionManager divisionManager;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (cylinder != null)
        {
            originalCylinderHeight = cylinder.localScale.y;
            originalCylinderPosition = cylinder.localPosition;
        }

        boxCollider = GetComponent<BoxCollider>();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (originalCylinderHeight > 0)
        {
            ResetTile();

            if (DivisionManager.Instance != null)
            {
                DivisionManager.Instance.ResetDivision();
            }
        }
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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCheckingInProgress)
        {
            StartCoroutine(CheckVerticalAfterDelay(other.gameObject));
        }
    }

    IEnumerator CheckVerticalAfterDelay(GameObject player)
    {
        isCheckingInProgress = true;

        yield return new WaitForSeconds(0.7f);

        bool isVertical = MoveCube.Instance != null
            ? MoveCube.Instance.IsInVerticalPosition()
            : IsPlayerVertical(player);

        if (isVertical)
        {
            PressedTile(player);
        }

        isCheckingInProgress = false;
    }

    private bool IsPlayerVertical(GameObject player)
    {
        return player.transform.up.y > 0.9f;
    }

    private void PressedTile(GameObject player)
    {

        if (buttonSound != null)
        {
            AudioSource.PlayClipAtPoint(buttonSound, transform.position, soundVolume);
        }

        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

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

        DivisionManager currentDivisionManager = DivisionManager.Instance;
        MoveCube playerScript = player.GetComponent<MoveCube>();

        if (currentDivisionManager != null && spawnPointA != null && spawnPointB != null && playerScript != null)
        {
            currentDivisionManager.DividePlayerAtPositions(
                spawnPointA.position,
                spawnPointB.position,
                playerScript.transform.rotation
            );
        }
        else
        {
            Debug.LogError($"[DivisionTile] {gameObject.name} - No se puede dividir el jugador. divisionManager={currentDivisionManager != null}, spawnA={spawnPointA != null}, spawnB={spawnPointB != null}, playerScript={playerScript != null}");
        }

        StartCoroutine(ResetCylinderAfterDelay());
    }

    IEnumerator ResetCylinderAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);

        if (cylinder != null)
        {
            cylinder.localScale = new Vector3(
                cylinder.localScale.x,
                originalCylinderHeight,
                cylinder.localScale.z
            );

            cylinder.localPosition = originalCylinderPosition;
        }

        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
    }

    public void ResetTile()
    {
        isCheckingInProgress = false;

        if (cylinder != null)
        {
            cylinder.localScale = new Vector3(
                cylinder.localScale.x,
                originalCylinderHeight,
                cylinder.localScale.z
            );

            cylinder.localPosition = originalCylinderPosition;
        }

        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
    }

    public static void ResetAllDivisionTiles()
    {
        DivisionTile[] allTiles = FindObjectsOfType<DivisionTile>();

        foreach (DivisionTile tile in allTiles)
        {
            if (tile != null)
            {
                tile.ResetTile();
            }
        }
    }

    private void OnDrawGizmos()
    {
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

        if (spawnPointA != null && spawnPointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(spawnPointA.position, spawnPointB.position);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.1f, new Vector3(1f, 0.2f, 1f));
    }
}