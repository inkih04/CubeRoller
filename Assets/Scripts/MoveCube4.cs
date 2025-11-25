using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class MoveCube : MonoBehaviour
{
    public static MoveCube Instance;

    [Header("Configuración Bloxorz")]
    public float rotSpeed = 300f;
    public float fallSpeed = 10f;

    [Header("Referencias")]
    public GameObject ghostPlayer;
    public AudioClip[] sounds;
    public AudioClip fallSound;
    public TMP_Text movesText;

    // Estado interno
    private bool isMoving = false;
    private bool isFalling = false;
    private int moveCount = 0;

    // Matemáticas de rotación
    private Vector3 pivot;
    private Vector3 rotAxis;
    private float degreesToRotate = 90f;
    private float currentRotated = 0f;
    private float rotationDirection = 0f;

    // Componentes
    InputAction moveAction;
    LayerMask groundLayerMask;
    BoxCollider boxCollider;
    Rigidbody rb;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();

        // Configuración física: Kinematic para que no lo mueva Unity, sino nosotros
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        if (ghostPlayer == null)
        {
            GameObject go = GameObject.FindWithTag("GhostPlayer");
            if (go != null) ghostPlayer = go;
        }
    }

    void Start()
    {
        // 1. Configurar Input
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
        }
        else
        {
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
        }

        // 2. Configurar Layer del Suelo
        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        if (groundLayerIndex == -1)
        {
            groundLayerMask = LayerMask.GetMask("Default");
        }
        else
        {
            groundLayerMask = LayerMask.GetMask("Ground");
        }
    }

    void Update()
    {
        if (isFalling)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
            return;
        }

        if (isMoving)
        {
            PerformRotation();
            return;
        }

        HandleInput();
    }

    void HandleInput()
    {
        if (!isGrounded())
        {
            StartFalling();
            return;
        }

        if (moveAction == null) return;

        Vector2 input = moveAction.ReadValue<Vector2>();

        if (Mathf.Abs(input.x) < 0.5f && Mathf.Abs(input.y) < 0.5f) return;

        Vector3 direction = Vector3.zero;
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            direction = input.x > 0 ? Vector3.right : Vector3.left;
        else
            direction = input.y > 0 ? Vector3.forward : Vector3.back;

        SimulateAndMove(direction);
    }

    void SimulateAndMove(Vector3 direction)
    {
        // Calculamos pivote usando el Ghost si existe, o el player si no
        Transform targetForCalculation = (ghostPlayer != null) ? ghostPlayer.transform : transform;

        if (ghostPlayer != null)
        {
            // Movemos el ghost a nuestra posición actual para simular desde ahí
            ghostPlayer.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }

        // Calculamos dónde estaría el pivote
        CalculatePivot(targetForCalculation, direction);

        // Iniciamos movimiento real
        StartRealRotation(direction);
    }

    void StartRealRotation(Vector3 dir)
    {
        isMoving = true;
        currentRotated = 0f;
        degreesToRotate = 90f;

        // Recalculamos el pivote sobre el objeto real para asegurar
        CalculatePivot(transform, dir);

        if (sounds != null && sounds.Length > 0)
            AudioSource.PlayClipAtPoint(sounds[UnityEngine.Random.Range(0, sounds.Length)], transform.position);
    }

    bool CalculatePivot(Transform targetTransform, Vector3 dir)
    {
        if (boxCollider == null) return false;

        // Medidas exactas del collider en este momento
        float distToBottom = boxCollider.bounds.extents.y;
        float distToEdge = 0f;

        if (Mathf.Abs(dir.x) > 0) distToEdge = boxCollider.bounds.extents.x;
        else distToEdge = boxCollider.bounds.extents.z;

        // Definimos el punto de rotación en el suelo
        pivot = targetTransform.position + (dir * distToEdge) + (Vector3.down * distToBottom);

        // Definimos el eje de rotación
        rotAxis = Vector3.Cross(Vector3.up, dir);
        rotationDirection = 1f;

        return true;
    }

    void PerformRotation()
    {
        float step = rotSpeed * Time.deltaTime;

        if (currentRotated + step > degreesToRotate)
        {
            // Fin del movimiento
            step = degreesToRotate - currentRotated;
            transform.RotateAround(pivot, rotAxis, step * rotationDirection);
            isMoving = false;

            // Solo redondeamos la rotación para que quede plano (0, 90, 180...)
            // PERO NO TOCAMOS LA POSICIÓN (No Grid Snap)
            Vector3 euler = transform.eulerAngles;
            euler.x = Mathf.Round(euler.x / 90) * 90;
            euler.y = Mathf.Round(euler.y / 90) * 90;
            euler.z = Mathf.Round(euler.z / 90) * 90;
            transform.rotation = Quaternion.Euler(euler);

            moveCount++;
            if (movesText != null) movesText.text = "Moves: " + moveCount;
        }
        else
        {
            // Durante el movimiento
            transform.RotateAround(pivot, rotAxis, step * rotationDirection);
            currentRotated += step;
        }
    }

    bool isGrounded()
    {
        if (boxCollider == null) return false;

        // Usamos el "Rayo desde el cielo" para que no falle nunca la detección
        float halfHeight = boxCollider.bounds.extents.y;
        Vector3 origin = transform.position + Vector3.up * 1.0f;
        float totalDist = 1.0f + halfHeight + 0.2f;

        Debug.DrawRay(origin, Vector3.down * totalDist, Color.cyan);

        return Physics.Raycast(origin, Vector3.down, totalDist, groundLayerMask);
    }

    void StartFalling()
    {
        isFalling = true;
        if (fallSound != null) AudioSource.PlayClipAtPoint(fallSound, transform.position);
    }

    private void OnDrawGizmos()
    {
        if (isMoving)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pivot, 0.1f);
        }
    }
}