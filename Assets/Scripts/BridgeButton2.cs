using UnityEngine;
using System.Collections;

public class BridgeButton2 : MonoBehaviour
{
    [SerializeField] private Transform cylinder;
    [SerializeField] private GameObject[] targetTiles; 
    [SerializeField] private GameObject[] oppositeTiles; 
    [SerializeField] private float cylinderDescendAmount = 0.1f;
    [SerializeField] private float cylinderResetTime = 1f;

    private float originalCylinderHeight;
    private Vector3 originalCylinderPosition;
    private BoxCollider boxCollider;
    private bool isPressed = false; 
    private bool canBePressed = true; 
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


        foreach (GameObject tile in targetTiles)
        {
            if (tile != null)
            {
                SetTileVisibility(tile, true);
            }
        }


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

        if (collision.CompareTag("Player") && canBePressed && !isPressed)
        {
            PressButton();
        }
    }

    private void OnTriggerExit(Collider collision)
    {

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

  
        if (cylinder != null)
        {
            Vector3 newScale = cylinder.localScale;
            newScale.y -= cylinderDescendAmount;
            cylinder.localScale = newScale;


            cylinder.localPosition = new Vector3(
                cylinder.localPosition.x,
                cylinder.localPosition.y - (cylinderDescendAmount / 2f),
                cylinder.localPosition.z
            );
        }


        ToggleTargetTiles();

        Debug.Log("Botón presionado: " + gameObject.name);
    }

    private IEnumerator ResetButtonAfterDelay()
    {

        yield return new WaitForSeconds(cylinderResetTime);


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
 
                bool isCurrentlyVisible = IsTileVisible(tile);


                SetTileVisibility(tile, !isCurrentlyVisible);

                Debug.Log("Tile " + tile.name + " - Visibilidad cambiada a: " + !isCurrentlyVisible);
            }
        }


        foreach (GameObject tile in oppositeTiles)
        {
            if (tile != null)
            {

                bool isCurrentlyVisible = IsTileVisible(tile);

                SetTileVisibility(tile, !isCurrentlyVisible);

                Debug.Log("Tile opuesto " + tile.name + " - Visibilidad cambiada a: " + !isCurrentlyVisible);
            }
        }
    }

    private bool IsTileVisible(GameObject tile)
    {
        MeshRenderer meshRenderer = tile.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            return meshRenderer.enabled;
        }

        return false;
    }

    private void SetTileVisibility(GameObject tile, bool visible)
    {

        MeshRenderer meshRenderer = tile.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = visible;
        }

        Collider tileCollider = tile.GetComponent<Collider>();
        if (tileCollider != null)
        {
            tileCollider.enabled = visible;
        }
    }

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

        foreach (GameObject tile in targetTiles)
        {
            if (tile != null)
            {
                SetTileVisibility(tile, true);
            }
        }

        foreach (GameObject tile in oppositeTiles)
        {
            if (tile != null)
            {
                SetTileVisibility(tile, false);
            }
        }
    }
}