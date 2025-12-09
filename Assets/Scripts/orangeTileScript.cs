using System.Collections;
using UnityEngine;

public class OrangeTileScript : MonoBehaviour
{
    [Header("Configuración de Caída")]
    [SerializeField] private float delayBeforeCheck = 0.7f;
    [SerializeField] private float fallDelay = 0.3f;
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private float destroyAfterSeconds = 3f;

    private bool isPlayerOn = false;
    private bool hasFallen = false;
    private Rigidbody parentRb;
    private GameObject parentObject;

    private void Start()
    {
        // Obtener el objeto padre
        parentObject = transform.parent != null ? transform.parent.gameObject : gameObject;

        // Buscar o añadir Rigidbody al padre
        parentRb = parentObject.GetComponent<Rigidbody>();
        if (parentRb == null)
        {
            parentRb = parentObject.AddComponent<Rigidbody>();
        }

        // Configurar Rigidbody para que no se caiga hasta que lo activemos
        parentRb.isKinematic = true;
        parentRb.useGravity = false;

        Debug.Log($"[OrangeTile] Configurado. Padre: {parentObject.name}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasFallen && !isPlayerOn)
        {
            Debug.Log("[OrangeTile] Player ha entrado en el tile.");
            isPlayerOn = true;
            StartCoroutine(CheckVerticalAndFall(other.gameObject));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOn = false;
        }
    }

    private IEnumerator CheckVerticalAndFall(GameObject player)
    {
        // Esperar antes de verificar la orientación
        yield return new WaitForSeconds(delayBeforeCheck);

        // Verificar si el jugador está en posición vertical
        bool isVertical = MoveCube.Instance != null
            ? MoveCube.Instance.IsInVerticalPosition()
            : IsPlayerVertical(player);

        Debug.Log($"[OrangeTile] ¿Vertical después de delay?: {isVertical}");

        if (isVertical && isPlayerOn)
        {
            Debug.Log($"<color=orange>[OrangeTile] ¡El suelo padre '{parentObject.name}' se va a caer! 🔥</color>");

            // Pequeño delay adicional antes de caer
            yield return new WaitForSeconds(fallDelay);

            // Activar la caída del padre
            MakeTileFall();
        }
        else
        {
            Debug.Log("<color=yellow>[OrangeTile] No estaba en vertical o el jugador salió, no se cae.</color>");
            isPlayerOn = false;
        }
    }

    private bool IsPlayerVertical(GameObject player)
    {
        return player.transform.up.y > 0.9f;
    }

    private void MakeTileFall()
    {
        hasFallen = true;

        // Activar física en el PADRE
        parentRb.isKinematic = false;
        parentRb.useGravity = true;

        // Opcional: aplicar una fuerza inicial hacia abajo para caída más dramática
        parentRb.AddForce(Vector3.down * fallSpeed, ForceMode.VelocityChange);

        // Destruir el PADRE después de unos segundos
        Destroy(parentObject, destroyAfterSeconds);
    }

    // Método opcional para resetear el tile (si usas un sistema de respawn)
    public void ResetTile()
    {
        hasFallen = false;
        isPlayerOn = false;

        if (parentRb != null)
        {
            parentRb.isKinematic = true;
            parentRb.useGravity = false;
            parentRb.linearVelocity = Vector3.zero;
            parentRb.angularVelocity = Vector3.zero;
        }

        parentObject.transform.rotation = Quaternion.identity;
    }
}