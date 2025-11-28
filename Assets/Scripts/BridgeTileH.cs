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
        // Verificar si el objeto que colisiona es el jugador y el tile no ha sido presionado
        if (collision.CompareTag("Player") && !hasBeenPressed)
        {
            PressedTile();
        }
    }

    private void PressedTile()
    {
        hasBeenPressed = true;

        // Desactivar el collider para que no se pueda presionar de nuevo
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        // Reducir la altura del cilindro en 0.1
        if (cylinder != null)
        {
            Vector3 newScale = cylinder.localScale;
            newScale.y -= 0.1f;
            cylinder.localScale = newScale;

            // Ajustar la posición Y para que el cilindro descienda desde su base
            cylinder.localPosition = new Vector3(
                cylinder.localPosition.x,
                cylinder.localPosition.y - 0.05f,
                cylinder.localPosition.z
            );
        }

        // Hacer visibles todos los tiles con tag GhostTileH
        GameObject[] ghostTiles = GameObject.FindGameObjectsWithTag("GhostTileH");
        Debug.Log("Tiles fantasma encontrados: " + ghostTiles.Length);

        if (ghostTiles.Length == 0)
        {
            Debug.LogWarning("ADVERTENCIA: No se encontraron tiles con tag 'GhostTileH'. Verifica que el tag esté asignado correctamente.");
        }

        foreach (GameObject tile in ghostTiles)
        {
            // Primero activar el gameObject si estaba desactivado
            if (!tile.activeSelf)
            {
                tile.SetActive(true);
                Debug.Log("Tile activado: " + tile.name);
            }

            // Activar el MeshRenderer directamente
            MeshRenderer meshRenderer = tile.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = true;
                Debug.Log("MeshRenderer activado en: " + tile.name);
            }

            // También activar renderers en hijos por si acaso
            Renderer[] renderers = tile.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true;
            }
        }

        Debug.Log("Tile presionado: " + gameObject.name);
    }

    // Opcional: resetear el tile manualmente
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