using UnityEngine;

public class BridgeTileH : MonoBehaviour
{
    [SerializeField] private Transform cylinder;

    private bool hasBeenPressed = false;
    private float originalCylinderHeight;
    private BoxCollider boxCollider;

    private void Start()
    {
        if (cylinder == null)
        {
            Debug.LogError("El cilindro no ha sido asignado en el inspector para " + gameObject.name);
            return;
        }

        originalCylinderHeight = cylinder.localScale.y;
        boxCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if ((collision.CompareTag("Player") || collision.CompareTag("HalfPlayer")) && !hasBeenPressed)
        {
            PressedTile();
        }
    }

    private void PressedTile()
    {
        hasBeenPressed = true;

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

        GameObject[] ghostTiles = GameObject.FindGameObjectsWithTag("GhostTileH");
        Debug.Log("Tiles fantasma encontrados: " + ghostTiles.Length);

        if (ghostTiles.Length == 0)
        {
            Debug.LogWarning("ADVERTENCIA: No se encontraron tiles con tag 'GhostTileH'. Verifica que el tag esté asignado correctamente.");
        }

        foreach (GameObject tile in ghostTiles)
        {
            if (!tile.activeSelf)
            {
                tile.SetActive(true);
                Debug.Log("Tile activado: " + tile.name);
            }

            MeshRenderer meshRenderer = tile.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = true;
                Debug.Log("MeshRenderer activado en: " + tile.name);
            }

            Renderer[] renderers = tile.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true;
            }
        }

        Debug.Log("Tile presionado: " + gameObject.name);
    }

    public void ResetTile()
    {
        hasBeenPressed = false;

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

        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
    }
}