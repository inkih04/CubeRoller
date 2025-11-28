using UnityEngine;

public class BridgeTileV : MonoBehaviour
{
    private bool hasBeenPressed = false;

    [SerializeField] private Transform cylinderTransform;

    private void OnTriggerStay(Collider other)
    {
        Debug.Log($"OnTriggerStay activado por: {other.gameObject.name}");

        // Verificar que sea el jugador y que esté en posición vertical
        if (other.CompareTag("Player") && !hasBeenPressed)
        {
            Debug.Log("Es el jugador y no ha sido presionado");

            // Verificar que el jugador esté en orientación vertical
            if (IsPlayerVertical(other.gameObject))
            {
                Debug.Log("Jugador en posición vertical, presionando tile...");
                PressedTile();
                hasBeenPressed = true;
            }
            else
            {
                Debug.Log("Jugador NO está en posición vertical");
            }
        }
    }

    private bool IsPlayerVertical(GameObject player)
    {
        // Verificar si el jugador está en posición vertical
        // Cuando está vertical, el eje Y local apunta hacia arriba (0, 1, 0)
        Vector3 upDirection = player.transform.up;
        Debug.Log($"Dirección UP del jugador: {upDirection}");

        // Considerar vertical si el Y es cercano a 1 (hacia arriba)
        bool isVertical = upDirection.y > 0.9f;
        Debug.Log($"¿Jugador vertical?: {isVertical}");

        return isVertical;
    }

    private void PressedTile()
    {
        Debug.Log("=== TILE PRESIONADO ===");

        // Reducir la altura del cilindro en 0.1 en el eje Y
        if (cylinderTransform != null)
        {
            Vector3 currentScale = cylinderTransform.localScale;
            Debug.Log($"Escala del cilindro antes: {currentScale}");

            currentScale.y -= 0.1f;
            cylinderTransform.localScale = currentScale;

            Debug.Log($"Escala del cilindro después: {cylinderTransform.localScale}");
        }
        else
        {
            Debug.LogError("¡El cilindro no está asignado en el inspector!");
        }

        // Hacer visibles todos los tiles con tag GhostTileV
        GameObject[] ghostTiles = GameObject.FindGameObjectsWithTag("GhostTileV");
        Debug.Log($"Se encontraron {ghostTiles.Length} tiles con tag GhostTileV");

        foreach (GameObject tile in ghostTiles)
        {
            tile.SetActive(true);
            Debug.Log($"Activado GhostTile: {tile.name}");

            // Si además quieres hacerlos visibles alterando el renderer:
            Renderer renderer = tile.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }
    }

    // Opcional: resetear el tile si necesitas
    public void ResetTile()
    {
        hasBeenPressed = false;
        // Aquí puedes restaurar la escala original del cilindro si es necesario
    }
}