using UnityEngine;
using System.Collections;

public class BridgeButton2 : MonoBehaviour
{
    [SerializeField] private Transform cylinder;
    [SerializeField] private GameObject[] targetTiles; // Array de tiles que se activarán/desactivarán
    [SerializeField] private GameObject[] oppositeTiles; // Array de tiles que harán lo contrario
    [SerializeField] private float cylinderDescendAmount = 0.1f;
    [SerializeField] private float cylinderResetTime = 1f;

    private float originalCylinderHeight;
    private Vector3 originalCylinderPosition;
    private BoxCollider boxCollider;
    private bool isPressed = false; // Si el botón está presionado
    private bool canBePressed = true; // Si el botón puede ser presionado
    private Coroutine resetCoroutine;

    private void Start()
    {
        if (cylinder == null)
        {
            Debug.LogError("El cilindro no ha sido asignado en el inspector para " + gameObject.name);
            return;
        }

        originalCylinderHeight = cylinder.localScale.y;
        originalCylinderPosition = cylinder.localPosition;
        boxCollider = GetComponent<BoxCollider>();

        // Asegurarse de que todos los tiles objetivo estén visibles al inicio
        foreach (GameObject tile in targetTiles)
        {
            if (tile != null)
            {
                SetTileVisibility(tile, true);
            }
        }

        // Asegurarse de que todos los tiles opuestos estén invisibles al inicio
        foreach (GameObject tile in oppositeTiles)
        {
            if (tile != null)
            {
                SetTileVisibility(tile, false);
            }
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        // Verificar si el objeto que colisiona es el jugador y el botón puede ser presionado
        if (collision.CompareTag("Player") && canBePressed && !isPressed)
        {
            PressButton();
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        // Cuando el jugador sale del botón, iniciar la cuenta regresiva para que suba
        if (collision.CompareTag("Player") && isPressed)
        {
            if (resetCoroutine != null)
            {
                StopCoroutine(resetCoroutine);
            }
            resetCoroutine = StartCoroutine(ResetButtonAfterDelay());
        }
    }

    private void PressButton()
    {
        isPressed = true;
        canBePressed = false;

        // Reducir la altura del cilindro
        if (cylinder != null)
        {
            Vector3 newScale = cylinder.localScale;
            newScale.y -= cylinderDescendAmount;
            cylinder.localScale = newScale;

            // Ajustar la posición Y para que el cilindro descienda desde su base
            cylinder.localPosition = new Vector3(
                cylinder.localPosition.x,
                cylinder.localPosition.y - (cylinderDescendAmount / 2f),
                cylinder.localPosition.z
            );
        }

        // Alternar visibilidad de los tiles objetivo
        ToggleTargetTiles();

        Debug.Log("Botón presionado: " + gameObject.name);
    }

    private IEnumerator ResetButtonAfterDelay()
    {
        // Esperar el tiempo especificado
        yield return new WaitForSeconds(cylinderResetTime);

        // Restaurar el cilindro a su posición original
        if (cylinder != null)
        {
            cylinder.localScale = new Vector3(
                cylinder.localScale.x,
                originalCylinderHeight,
                cylinder.localScale.z
            );
            cylinder.localPosition = originalCylinderPosition;
        }

        isPressed = false;
        canBePressed = true;

        Debug.Log("Botón reseteado: " + gameObject.name);
    }

    private void ToggleTargetTiles()
    {
        foreach (GameObject tile in targetTiles)
        {
            if (tile != null)
            {
                // Verificar si el tile está actualmente visible
                bool isCurrentlyVisible = IsTileVisible(tile);

                // Invertir su visibilidad
                SetTileVisibility(tile, !isCurrentlyVisible);

                Debug.Log("Tile " + tile.name + " - Visibilidad cambiada a: " + !isCurrentlyVisible);
            }
        }

        // Hacer lo contrario con los tiles opuestos
        foreach (GameObject tile in oppositeTiles)
        {
            if (tile != null)
            {
                // Verificar si el tile está actualmente visible
                bool isCurrentlyVisible = IsTileVisible(tile);

                // Invertir su visibilidad (será opuesto a targetTiles)
                SetTileVisibility(tile, !isCurrentlyVisible);

                Debug.Log("Tile opuesto " + tile.name + " - Visibilidad cambiada a: " + !isCurrentlyVisible);
            }
        }
    }

    private bool IsTileVisible(GameObject tile)
    {
        // Verificar si el MeshRenderer está activado
        MeshRenderer meshRenderer = tile.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            return meshRenderer.enabled;
        }

        return false;
    }

    private void SetTileVisibility(GameObject tile, bool visible)
    {
        // Activar/desactivar el MeshRenderer directamente
        MeshRenderer meshRenderer = tile.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = visible;
        }

        // Activar/desactivar el collider del tile según su visibilidad
        Collider tileCollider = tile.GetComponent<Collider>();
        if (tileCollider != null)
        {
            tileCollider.enabled = visible;
        }
    }

    // Método público para resetear manualmente el botón
    public void ManualReset()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        if (cylinder != null)
        {
            cylinder.localScale = new Vector3(
                cylinder.localScale.x,
                originalCylinderHeight,
                cylinder.localScale.z
            );
            cylinder.localPosition = originalCylinderPosition;
        }

        isPressed = false;
        canBePressed = true;

        // Restaurar todos los tiles a visible
        foreach (GameObject tile in targetTiles)
        {
            if (tile != null)
            {
                SetTileVisibility(tile, true);
            }
        }

        // Restaurar todos los tiles opuestos a invisible
        foreach (GameObject tile in oppositeTiles)
        {
            if (tile != null)
            {
                SetTileVisibility(tile, false);
            }
        }
    }
}