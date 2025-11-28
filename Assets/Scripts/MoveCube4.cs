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

    private bool isMoving = false;
    private bool isFalling = false;
    private int moveCount = 0;

    private Vector3 pivot;
    private Vector3 rotAxis;
    private float degreesToRotate = 90f;
    private float currentRotated = 0f;
    private float rotationDirection = 0f;

    InputAction moveAction;
    LayerMask groundLayerMask;
    BoxCollider boxCollider;
    Rigidbody rb;

    private Vector2 lastInput = Vector2.zero;
    private bool inputProcessed = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else { Destroy(gameObject); return; }

        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        if (ghostPlayer == null)
        {
            GameObject go = GameObject.FindWithTag("GhostPlayer");
            if (go != null) ghostPlayer = go;
        }
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            return;
        }

        GameObject spawnPoint = GameObject.FindGameObjectWithTag("Respawn");

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
            transform.rotation = spawnPoint.transform.rotation;

            isMoving = false;
            isFalling = false;
        }
        else
        {
            Debug.LogError("¡ERROR! No se encontró un SpawnPoint con el Tag 'Respawn' en la escena: " + scene.name);
        }



        GameObject counterGO = GameObject.FindGameObjectWithTag("MoveCounter");

        if (counterGO != null)
        {
            TMP_Text newMovesText = counterGO.GetComponent<TMP_Text>();

            if (newMovesText != null)
            {
                movesText = newMovesText;

                movesText.text = "Moves: " + moveCount;

                Debug.Log("Contador re-asignado exitosamente en la escena: " + scene.name);
            }
            else
            {
                Debug.LogError("El objeto con el tag 'MoveCounter' no tiene un componente TMP_Text.");
            }
        }
        else
        {
            Debug.LogWarning("No se encontró un objeto con el tag 'MoveCounter' en la escena: " + scene.name);
            movesText = null; 
        }
    }

    void Start()
    {
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
        if (isMoving)
        {
            inputProcessed = false; 
            return;
        }

        if (!isGrounded())
        {
            StartFalling();
            return;
        }

        if (moveAction == null) return;

        Vector2 input = moveAction.ReadValue<Vector2>();

        bool hasInput = Mathf.Abs(input.x) > 0.5f || Mathf.Abs(input.y) > 0.5f;

        if (!hasInput)
        {
            inputProcessed = false;
            return;
        }

        if (inputProcessed) return;

        inputProcessed = true;

        Vector3 direction = Vector3.zero;
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            direction = input.x > 0 ? Vector3.right : Vector3.left;
        else
            direction = input.y > 0 ? Vector3.forward : Vector3.back;

        SimulateAndMove(direction);
    }

    void SimulateAndMove(Vector3 direction)
    {
        if (ghostPlayer != null)
        {
            ghostPlayer.transform.SetPositionAndRotation(transform.position, transform.rotation);


            CalculatePivot(ghostPlayer.transform, direction);

            ghostPlayer.transform.RotateAround(pivot, rotAxis, 90f * rotationDirection);
        }
        else
        {
            CalculatePivot(transform, direction);
        }

        StartRealRotation(direction);
    }

    void StartRealRotation(Vector3 dir)
    {
        isMoving = true;
        currentRotated = 0f;
        degreesToRotate = 90f;


        if (sounds != null && sounds.Length > 0)
            AudioSource.PlayClipAtPoint(sounds[UnityEngine.Random.Range(0, sounds.Length)], transform.position);
    }

    bool CalculatePivot(Transform targetTransform, Vector3 dir)
    {
        if (boxCollider == null) return false;

        float distToBottom = boxCollider.bounds.extents.y;
        float distToEdge = 0f;

        if (Mathf.Abs(dir.x) > 0) distToEdge = boxCollider.bounds.extents.x;
        else distToEdge = boxCollider.bounds.extents.z;


        pivot = targetTransform.position + (dir * distToEdge) + (Vector3.down * distToBottom);


        rotAxis = Vector3.Cross(Vector3.up, dir);
        rotationDirection = 1f;

        return true;
    }

    void PerformRotation()
    {
        float step = rotSpeed * Time.deltaTime;

        if (currentRotated + step > degreesToRotate)
        {
            step = degreesToRotate - currentRotated;
            transform.RotateAround(pivot, rotAxis, step * rotationDirection);
            isMoving = false;

            if (ghostPlayer != null)
            {
                transform.position = ghostPlayer.transform.position;
                transform.rotation = ghostPlayer.transform.rotation;
            }
            else
            {
                Vector3 euler = transform.eulerAngles;
                euler.x = Mathf.Round(euler.x / 90) * 90;
                euler.y = Mathf.Round(euler.y / 90) * 90;
                euler.z = Mathf.Round(euler.z / 90) * 90;
                transform.rotation = Quaternion.Euler(euler);
            }

            moveCount++;
            if (movesText != null) movesText.text = "Moves: " + moveCount;
        }
        else
        {
            transform.RotateAround(pivot, rotAxis, step * rotationDirection);
            currentRotated += step;
        }
    }


    bool isGrounded()
    {
        if (boxCollider == null) return false;

        if (isMoving) return true;

        float halfHeight = boxCollider.bounds.extents.y;
        Vector3 origin = transform.position;
        float totalDist = halfHeight + 0.2f;

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