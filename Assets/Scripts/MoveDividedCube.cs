using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveDividedCube : MonoBehaviour
{
    [Header("Estado")]
    public bool isActive = false;
    public MoveDividedCube otherHalf;

    [Header("Configuración")]
    public float rotSpeed = 300f;
    public float fallSpeed = 10f;

    [Header("Audio")]
    public AudioClip[] sounds;
    public AudioClip fallSound;

    [HideInInspector] public bool bMoving = false;
    [HideInInspector] public bool bFalling = false;

    private InputAction moveAction;
    private LayerMask layerMask;

    private Vector3 rotPoint, rotAxis;
    private float rotRemainder;
    private float rotDir;

    void Start()
    {
        // Configurar el input
        moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        moveAction.Enable();

        // Configurar layer mask
        layerMask = LayerMask.GetMask("Ground");
    }

    void OnDestroy()
    {
        if (moveAction != null)
        {
            moveAction.Disable();
            moveAction.Dispose();
        }
    }

    bool IsGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.0f, layerMask))
            return true;
        return false;
    }

    void Update()
    {
        if (bFalling)
        {
            // Caída
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

            // Respawn si cae muy bajo
            if (transform.position.y < -5f)
            {
                GameObject spawnPoint = GameObject.FindGameObjectWithTag("Respawn");
                if (spawnPoint != null)
                {
                    transform.position = spawnPoint.transform.position;
                    transform.rotation = spawnPoint.transform.rotation;
                    bFalling = false;
                }
            }
        }
        else if (bMoving)
        {
            // Rotación
            float amount = rotSpeed * Time.deltaTime;
            if (amount > rotRemainder)
            {
                transform.RotateAround(rotPoint, rotAxis, rotRemainder * rotDir);
                bMoving = false;

                // Ajustar rotación
                Vector3 euler = transform.eulerAngles;
                euler.x = Mathf.Round(euler.x / 90) * 90;
                euler.y = Mathf.Round(euler.y / 90) * 90;
                euler.z = Mathf.Round(euler.z / 90) * 90;
                transform.rotation = Quaternion.Euler(euler);
            }
            else
            {
                transform.RotateAround(rotPoint, rotAxis, amount * rotDir);
                rotRemainder -= amount;
            }
        }
        else if (isActive) // Solo procesar input si esta mitad está activa
        {
            // Verificar si está en el suelo
            if (!IsGrounded())
            {
                bFalling = true;
                if (fallSound != null)
                    AudioSource.PlayClipAtPoint(fallSound, transform.position, 1.5f);
            }
            else
            {
                // Leer input
                Vector2 dir = moveAction.ReadValue<Vector2>();
                if (Mathf.Abs(dir.x) > 0.5f || Mathf.Abs(dir.y) > 0.5f)
                {
                    HandleMovement(dir);
                }
            }
        }
    }

    void HandleMovement(Vector2 dir)
    {
        bMoving = true;

        // Reproducir sonido
        if (sounds != null && sounds.Length > 0)
        {
            int iSound = UnityEngine.Random.Range(0, sounds.Length);
            AudioSource.PlayClipAtPoint(sounds[iSound], transform.position, 1.0f);
        }

        // Determinar dirección de movimiento
        Vector3 moveDir = Vector3.zero;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            moveDir = dir.x > 0 ? Vector3.right : Vector3.left;
        }
        else
        {
            moveDir = dir.y > 0 ? Vector3.forward : Vector3.back;
        }

        // Calcular pivote de rotación
        CalculatePivot(moveDir);
    }

    void CalculatePivot(Vector3 direction)
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;

        float distToBottom = box.bounds.extents.y;
        float distToEdge = 0f;

        if (Mathf.Abs(direction.x) > 0)
            distToEdge = box.bounds.extents.x;
        else
            distToEdge = box.bounds.extents.z;

        rotPoint = transform.position + (direction * distToEdge) + (Vector3.down * distToBottom);
        rotAxis = Vector3.Cross(Vector3.up, direction);
        rotDir = 1f;
        rotRemainder = 90f;
    }

    void OnDrawGizmos()
    {
        if (bMoving && rotPoint != Vector3.zero)
        {
            Gizmos.color = isActive ? Color.green : Color.yellow;
            Gizmos.DrawSphere(rotPoint, 0.08f);
        }
    }
}