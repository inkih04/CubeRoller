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
    public float fallRotationSpeed = 180f;
    public float maxFallRotation = 90f; // Máxima rotación antes de solo caer

    [Header("Referencias")]
    public GameObject ghostPlayer;
    public AudioClip[] sounds;
    public AudioClip fallSound;
    public TMP_Text movesText;

    private bool isMoving = false;
    private bool isFalling = false;
    private float fallRotationAmount = 0f; // Cuánto ha rotado durante la caída
    private int moveCount = 0;

    private Vector3 pivot;
    private Vector3 rotAxis;
    private float degreesToRotate = 90f;
    private float currentRotated = 0f;
    private float rotationDirection = 0f;

    // Variables para la animación de caída
    private Vector3 fallPivot;
    private Vector3 fallRotationAxis;
    private float fallRotationDirection = 1f;

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
            fallRotationAmount = 0f;
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
            // Animación de caída con rotación limitada
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

            if (fallRotationAmount < maxFallRotation)
            {
                float rotStep = fallRotationSpeed * Time.deltaTime;
                if (fallRotationAmount + rotStep > maxFallRotation)
                {
                    rotStep = maxFallRotation - fallRotationAmount;
                }
                transform.RotateAround(fallPivot, fallRotationAxis, rotStep * fallRotationDirection);
                fallRotationAmount += rotStep;
            }
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

        if (!IsGroundedAdvanced())
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

    // Nueva función mejorada para detectar si está en el suelo
    bool IsGroundedAdvanced()
    {
        if (boxCollider == null) return false;
        if (isMoving) return true;

        // Determinar si está en horizontal o vertical
        bool isHorizontal = IsInHorizontalPosition();

        if (isHorizontal)
        {
            // En horizontal: verificar ambas mitades del rectángulo
            return CheckBothHalvesGrounded();
        }
        else
        {
            // En vertical: usar el raycast desde el centro (comportamiento original)
            return CheckCenterGrounded();
        }
    }

    // Determina si el cubo está en posición horizontal usando los contact points
    bool IsInHorizontalPosition()
    {
        // Usamos los bounds locales del collider para determinar la orientación
        Vector3 localSize = boxCollider.size;

        // Calculamos las dimensiones en el espacio mundial considerando la rotación
        Vector3 worldX = transform.TransformVector(new Vector3(localSize.x, 0, 0));
        Vector3 worldY = transform.TransformVector(new Vector3(0, localSize.y, 0));
        Vector3 worldZ = transform.TransformVector(new Vector3(0, 0, localSize.z));

        float sizeX = worldX.magnitude;
        float sizeY = worldY.magnitude;
        float sizeZ = worldZ.magnitude;

        // Está horizontal si la altura (Y) es menor que el máximo horizontal
        // Con escala (1,1,2), en horizontal Y?1, y X o Z?2
        // En vertical Y?2, y X y Z?1
        float height = sizeY;
        float maxHorizontal = Mathf.Max(sizeX, sizeZ);

        // Está horizontal cuando la altura es la dimensión pequeña
        return height < maxHorizontal * 0.8f;
    }

    // Verifica si el centro está tocando el suelo (para posición vertical)
    bool CheckCenterGrounded()
    {
        float halfHeight = boxCollider.bounds.extents.y;
        Vector3 origin = transform.position;
        float totalDist = halfHeight + 0.15f;

        Debug.DrawRay(origin, Vector3.down * totalDist, Color.cyan, 0.1f);
        return Physics.Raycast(origin, Vector3.down, totalDist, groundLayerMask);
    }

    // Verifica si ambas mitades del rectángulo están tocando el suelo
    bool CheckBothHalvesGrounded()
    {
        // Obtener la dirección del eje largo del cubo
        Vector3 longAxisDirection = GetLongAxisDirection();

        // La distancia desde el centro a cada cubo es 0.5 unidades (la mitad de 1 unidad)
        // ya que el cubo tiene escala 2 en un eje, pero queremos el centro de cada "cubo"
        float offsetDistance = 0.5f;

        // Posiciones de los centros de cada cubo
        Vector3 center1 = transform.position + longAxisDirection * offsetDistance;
        Vector3 center2 = transform.position - longAxisDirection * offsetDistance;

        // Distancia de raycast
        float halfHeight = boxCollider.bounds.extents.y;
        float rayDistance = halfHeight + 0.15f;

        // Trazar rayos desde cada centro
        bool half1Grounded = Physics.Raycast(center1, Vector3.down, rayDistance, groundLayerMask);
        bool half2Grounded = Physics.Raycast(center2, Vector3.down, rayDistance, groundLayerMask);

        // Debug visual
        Debug.DrawRay(center1, Vector3.down * rayDistance, half1Grounded ? Color.green : Color.red, 0.1f);
        Debug.DrawRay(center2, Vector3.down * rayDistance, half2Grounded ? Color.green : Color.red, 0.1f);

        // Si alguna mitad no está tocando el suelo, el cubo debe caer
        bool isGrounded = half1Grounded && half2Grounded;

        // Si solo una mitad está en el suelo, calcular el pivote para la animación de caída
        if (!isGrounded && (half1Grounded || half2Grounded))
        {
            // Determinar cuál mitad está en el aire
            Vector3 airborneCenter = half2Grounded ? center1 : center2;
            Vector3 groundedCenter = half1Grounded ? center1 : center2;

            CalculateFallPivot(airborneCenter, groundedCenter, longAxisDirection);
        }

        return isGrounded;
    }

    // Obtiene la dirección del eje largo del cubo en espacio mundial
    Vector3 GetLongAxisDirection()
    {
        // Con escala (1, 1, 2), el eje Z local es el largo
        Vector3 localLongAxis = new Vector3(0, 0, 1);

        // Transformarlo al espacio mundial
        Vector3 worldLongAxis = transform.TransformDirection(localLongAxis);

        // Proyectar en el plano horizontal (XZ) y normalizar
        worldLongAxis.y = 0;
        worldLongAxis.Normalize();

        return worldLongAxis;
    }

    // Calcula el pivote para la animación de caída desde el cubo en el aire
    void CalculateFallPivot(Vector3 airborneCenter, Vector3 groundedCenter, Vector3 longAxis)
    {
        float halfHeight = boxCollider.bounds.extents.y;

        // El pivote está en el borde inferior del cubo que está en el aire
        // Esto es en el punto donde los dos cubos se tocarían
        fallPivot = airborneCenter + Vector3.down * halfHeight;

        // El eje de rotación es perpendicular al eje largo en el plano horizontal
        fallRotationAxis = Vector3.Cross(Vector3.up, longAxis).normalized;

        // Determinar la dirección de rotación para que caiga hacia afuera
        // El cubo debe rotar alejándose del cubo que está en el suelo
        Vector3 directionToAirborne = (airborneCenter - groundedCenter).normalized;

        // Producto cruz para determinar si la rotación debe ser positiva o negativa
        Vector3 testRotation = Vector3.Cross(fallRotationAxis, Vector3.down);
        float dotProduct = Vector3.Dot(testRotation, directionToAirborne);

        // INVERTIDO: para rotar hacia afuera del mapa
        fallRotationDirection = dotProduct > 0 ? -1f : 1f;

        Debug.Log($"Fall Pivot calculado: {fallPivot}, Eje: {fallRotationAxis}, Dirección: {fallRotationDirection}");
    }

    void StartFalling()
    {
        isFalling = true;
        fallRotationAmount = 0f;
        if (fallSound != null)
            AudioSource.PlayClipAtPoint(fallSound, transform.position);
    }

    private void OnDrawGizmos()
    {
        if (isMoving && pivot != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pivot, 0.1f);
        }

        if (isFalling && fallPivot != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(fallPivot, 0.15f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(fallPivot, fallRotationAxis * 0.5f);
        }
    }
}