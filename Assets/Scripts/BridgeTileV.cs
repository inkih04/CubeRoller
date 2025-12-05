using System.Collections;
using UnityEngine;

public class BridgeTileV : MonoBehaviour
{
    private bool hasBeenPressed = false;

[SerializeField] private Transform cylinderTransform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenPressed)
        {
            Debug.Log("[BridgeTileV] Player ha entrado en trigger.");
            StartCoroutine(CheckVerticalAfterDelay(other.gameObject));
        }
    }

    IEnumerator CheckVerticalAfterDelay(GameObject player)
    {
        yield return new WaitForSeconds(0.3f);

        bool isVertical = MoveCube.Instance != null
            ? MoveCube.Instance.IsInVerticalPosition()
            : IsPlayerVertical(player);

        Debug.Log($"[BridgeTileV] ¿Vertical después de delay?: {isVertical}");

        if (isVertical)
        {
            Debug.Log("<color=cyan>[BridgeTileV] Tile activado correctamente ?</color>");
            hasBeenPressed = true;
            PressedTile();
        }
        else
        {
            Debug.Log("<color=yellow>[BridgeTileV] No estaba en vertical, no se activa.</color>");
        }
    }

    private bool IsPlayerVertical(GameObject player)
    {
        return player.transform.up.y > 0.9f;
    }

    private void PressedTile()
    {
        Debug.Log("=== TILE PRESIONADO (BridgeTileV) ===");

        if (cylinderTransform != null)
        {
            Vector3 currentScale = cylinderTransform.localScale;
            currentScale.y -= 0.1f;
            cylinderTransform.localScale = currentScale;
        }

        GameObject[] ghostTiles = GameObject.FindGameObjectsWithTag("GhostTileV");

        foreach (GameObject tile in ghostTiles)
        {
            tile.SetActive(true);
            Renderer renderer = tile.GetComponent<Renderer>();

            if (renderer != null)
                renderer.enabled = true;
        }
    }

    public void ResetTile()
    {
        hasBeenPressed = false;
    }

}
