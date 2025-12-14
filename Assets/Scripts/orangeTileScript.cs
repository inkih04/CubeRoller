using System.Collections;
using UnityEngine;

public class OrangeTileScript : MonoBehaviour
{
    [Header("Configuración de Caída")]
    [SerializeField] private float delayBeforeCheck = 0.7f;
    [SerializeField] private float fallDelay = 0.3f;
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private float destroyAfterSeconds = 3f;

    [Header("Audio")]
    [SerializeField] private AudioClip buttonSound;
    [SerializeField][Range(0f, 1f)] private float soundVolume = 1f;

    private bool isPlayerOn = false;
    private bool hasFallen = false;
    private Rigidbody parentRb;
    private GameObject parentObject;

    private void Start()
    {
        parentObject = transform.parent != null ? transform.parent.gameObject : gameObject;


        parentRb = parentObject.GetComponent<Rigidbody>();
        if (parentRb == null)
        {
            parentRb = parentObject.AddComponent<Rigidbody>();
        }

  
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

        yield return new WaitForSeconds(delayBeforeCheck);


        bool isVertical = MoveCube.Instance != null
            ? MoveCube.Instance.IsInVerticalPosition()
            : IsPlayerVertical(player);

        Debug.Log($"[OrangeTile] ¿Vertical después de delay?: {isVertical}");

        if (isVertical && isPlayerOn)
        {
            Debug.Log($"<color=orange>[OrangeTile] ¡El suelo padre '{parentObject.name}' se va a caer! 🔥</color>");


            yield return new WaitForSeconds(fallDelay);


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
        if (buttonSound != null)
        {
            AudioSource.PlayClipAtPoint(buttonSound, transform.position, soundVolume);
        }

        hasFallen = true;


        parentRb.isKinematic = false;
        parentRb.useGravity = true;


        parentRb.AddForce(Vector3.down * fallSpeed, ForceMode.VelocityChange);


        Destroy(parentObject, destroyAfterSeconds);
    }


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