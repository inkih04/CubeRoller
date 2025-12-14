using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class MoveCube : MonoBehaviour
{
    public static MoveCube Instance;

    [SerializeField] private float inputCooldown = 0.05f;
    private float inputCooldownTimer = 0f;
    private bool waitingForCooldown = false;


    [Header("Configuración Bloxorz")]
    public float rotSpeed = 300f;
    public float fallSpeed = 10f;
    public float fallRotationSpeed = 180f;
    public float maxFallRotation = 70f;
    public float fallRespawnTime = 5f;

    [Header("Debug")]
    public bool showOrientationDebug = true;
    private float debugUpdateInterval = 0.5f;
    private float debugTimer = 0f;
    private string lastOrientationDebug = "";

    [Header("Referencias")]
    public GameObject ghostPlayer;
    public AudioClip[] sounds;
    public AudioClip fallSound;
    public TMP_Text movesText;

    private bool isMoving = false;
    private bool isFalling = false;
    private bool isVictory = false; 
    private bool controlsActive = true; 
    private float fallRotationAmount = 0f;
    private float fallTimer = 0f;
    private int moveCount = 0;

    private Vector3 pivot;
    private Vector3 rotAxis;
    private float degreesToRotate = 90f;
    private float currentRotated = 0f;
    private float rotationDirection = 0f;
    private float targetX, targetZ; 

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
        if (scene.name == "MainMenu") return;

        // Resetear estados
        isVictory = false;
        isMoving = false;
        isFalling = false;
        fallRotationAmount = 0f;
        fallTimer = 0f;
        controlsActive = true;


        transform.position = Vector3.up * 50f;
        this.enabled = true;

        // Ocultar jugador inicialmente
        HidePlayer();

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
        if (ghostPlayer != null)
        {
            Renderer[] renderers = ghostPlayer.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers) r.enabled = false;
        }

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

        rb.isKinematic = true;
    }


    public void HidePlayer()
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allRenderers) r.enabled = false;
        rb.isKinematic = true;
        controlsActive = false;
    }

    public void SpawnPlayerFromSky(float height)
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("Respawn");
        if (spawnPoint != null)
        {
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in allRenderers) r.enabled = true;

            targetX = spawnPoint.transform.position.x;
            targetZ = spawnPoint.transform.position.z;
            transform.position = spawnPoint.transform.position + Vector3.up * height;
            transform.rotation = spawnPoint.transform.rotation;

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

            StartCoroutine(WaitForLanding());
        }
    }

    IEnumerator WaitForLanding()
    {
        yield return new WaitForSeconds(0.1f);
        float distToGround = boxCollider.bounds.extents.y + 0.1f;
        while (!Physics.Raycast(transform.position, Vector3.down, distToGround, groundLayerMask))
            yield return null;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f, groundLayerMask))
        {
            float perfectY = hit.point.y + boxCollider.bounds.extents.y;
            transform.position = new Vector3(targetX, perfectY, targetZ);
        }

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        Vector3 euler = transform.eulerAngles;
        euler.x = Mathf.Round(euler.x / 90) * 90;
        euler.y = Mathf.Round(euler.y / 90) * 90;
        euler.z = Mathf.Round(euler.z / 90) * 90;
        transform.rotation = Quaternion.Euler(euler);

        controlsActive = true;
        isMoving = false;
    }

    public void SetPlayerControl(bool active)
    {
        controlsActive = active;
        if (!active) isMoving = false;
    }


    public bool IsStopped()
    {
        return !isMoving && !isFalling && rb.linearVelocity.sqrMagnitude < 0.01f;
    }

    public bool IsVertical()
    {
        float tolerance = 0.1f;
        return boxCollider.bounds.size.y > (boxCollider.bounds.size.x + tolerance) &&
               boxCollider.bounds.size.y > (boxCollider.bounds.size.z + tolerance);
    }

    public void FallIntoHole(string nextLevel)
    {
        StartCoroutine(FallWinSequence(nextLevel));
    }

    IEnumerator FallWinSequence(string nextLevel)
    {
        isVictory = true;
        isFalling = true;
        SetPlayerControl(false);

        if (fallSound != null)
            AudioSource.PlayClipAtPoint(fallSound, transform.position);

        boxCollider.enabled = false;

        float dropDistance = 40f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.down * dropDistance;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        LevelSequenceManager manager = FindObjectOfType<LevelSequenceManager>();
        if (manager != null)
            manager.LoadNextLevel(nextLevel);
        else
            SceneManager.LoadScene(nextLevel);

        boxCollider.enabled = true;
    }


    void Update()
    {
        if (!isVictory && transform.position.y < -5f)
        {
            LevelSequenceManager manager = FindObjectOfType<LevelSequenceManager>();
            if (manager != null)
            {
                HidePlayer();
                manager.RestartLevel();
                enabled = false;
                return;
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (!isVictory && transform.position.y < -5f)
        {

            if (DivisionManager.Instance != null)
            {
                DivisionManager.Instance.ResetDivision();
            }

            DivisionTile.ResetAllDivisionTiles();

            LevelSequenceManager manager = FindObjectOfType<LevelSequenceManager>();
            if (manager != null)
            {
                HidePlayer();
                manager.RestartLevel();
                enabled = false;
                return;
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (showOrientationDebug && !isMoving && boxCollider != null)
        {
            debugTimer += Time.deltaTime;
            if (debugTimer >= debugUpdateInterval)
            {
                debugTimer = 0f;

                bool isHorizontal = IsInHorizontalPosition();
                bool isVertical = !isHorizontal;

                Vector3 worldSize = boxCollider.bounds.size;
                float worldHeight = worldSize.y;
                float worldMaxHorizontal = Mathf.Max(worldSize.x, worldSize.z);

                string orientationText = isVertical ? "VERTICAL ✓" : "HORIZONTAL ↔";
                string debugMsg = $"[PLAYER] Orientación: {orientationText} | " +
                                $"Bounds.size: (X:{worldSize.x:F2}, Y:{worldSize.y:F2}, Z:{worldSize.z:F2}) | " +
                                $"Altura: {worldHeight:F2} vs Ancho: {worldMaxHorizontal:F2} | " +
                                $"Rotación: {transform.eulerAngles}";

                if (debugMsg != lastOrientationDebug)
                {
                    Debug.Log(debugMsg);
                    lastOrientationDebug = debugMsg;
                }
            }
        }

        if (isVictory) return;

        if (isFalling)
        {
            fallTimer += Time.deltaTime;

            if (fallTimer >= fallRespawnTime)
            {
                RespawnPlayer();
                return;
            }

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

        if (waitingForCooldown)
        {
            inputCooldownTimer -= Time.deltaTime;
            if (inputCooldownTimer <= 0f)
            {
                waitingForCooldown = false;
                inputProcessed = false;
            }
            return;
        }

        if (controlsActive)
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        if (isMoving)
        {
            inputProcessed = false;
            return;
        }

        if (waitingForCooldown)
        {
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

            waitingForCooldown = true;
            inputCooldownTimer = inputCooldown;

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

    bool IsGroundedAdvanced()
    {
        if (boxCollider == null) return false;
        if (isMoving) return true;

        bool isHorizontal = IsInHorizontalPosition();

        if (isHorizontal)
        {
            return CheckBothHalvesGrounded();
        }
        else
        {
            return CheckCenterGrounded();
        }
    }

    bool IsInHorizontalPosition()
    {
        if (boxCollider == null) return false;

        Vector3 worldSize = boxCollider.bounds.size;

        float worldHeight = worldSize.y;
        float worldMaxHorizontal = Mathf.Max(worldSize.x, worldSize.z);

        bool isHorizontal = worldHeight < worldMaxHorizontal;

        return isHorizontal;
    }

    public bool IsInVerticalPosition()
    {
        return !IsInHorizontalPosition();
    }

    public bool IsFalling()
    {
        return isFalling;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    bool CheckCenterGrounded()
    {
        float halfHeight = boxCollider.bounds.extents.y;
        Vector3 origin = transform.position;
        float totalDist = halfHeight + 0.15f;

        Debug.DrawRay(origin, Vector3.down * totalDist, Color.cyan, 0.1f);

        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.down, out hit, totalDist, groundLayerMask))
        {
            if (hit.collider != null && hit.collider.CompareTag("WinTile") && hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                Debug.Log("[MoveCube] Detectada WinTile debajo en layer Ground (posición VERTICAL) -> se permite caer.");
                return false;
            }
            return true;
        }

        return false;
    }

    bool CheckBothHalvesGrounded()
    {
        Vector3 longAxisDirection = GetLongAxisDirection();

        float offsetDistance = 0.5f;

        Vector3 center1 = transform.position + longAxisDirection * offsetDistance;
        Vector3 center2 = transform.position - longAxisDirection * offsetDistance;

        float halfHeight = boxCollider.bounds.extents.y;
        float rayDistance = halfHeight + 0.15f;

        RaycastHit hit1, hit2;
        bool half1Grounded = Physics.Raycast(center1, Vector3.down, out hit1, rayDistance, groundLayerMask);
        bool half2Grounded = Physics.Raycast(center2, Vector3.down, out hit2, rayDistance, groundLayerMask);

        bool half1IsWinTile = half1Grounded && hit1.collider != null && hit1.collider.CompareTag("WinTile");
        bool half2IsWinTile = half2Grounded && hit2.collider != null && hit2.collider.CompareTag("WinTile");

        if (half1IsWinTile) half1Grounded = true;
        if (half2IsWinTile) half2Grounded = true;

        Debug.DrawRay(center1, Vector3.down * rayDistance, half1Grounded ? Color.green : Color.red, 0.1f);
        Debug.DrawRay(center2, Vector3.down * rayDistance, half2Grounded ? Color.green : Color.red, 0.1f);

        bool isGrounded = half1Grounded && half2Grounded;

        if (!isGrounded && (half1Grounded || half2Grounded))
        {
            Vector3 airborneCenter = half2Grounded ? center1 : center2;
            Vector3 groundedCenter = half1Grounded ? center1 : center2;

            CalculateFallPivot(airborneCenter, groundedCenter, longAxisDirection);
        }

        return isGrounded;
    }

    Vector3 GetLongAxisDirection()
    {
        Vector3 localLongAxis = new Vector3(0, 0, 1);

        Vector3 worldLongAxis = transform.TransformDirection(localLongAxis);

        worldLongAxis.y = 0;
        worldLongAxis.Normalize();

        return worldLongAxis;
    }

    void CalculateFallPivot(Vector3 airborneCenter, Vector3 groundedCenter, Vector3 longAxis)
    {
        float halfHeight = boxCollider.bounds.extents.y;

        fallPivot = airborneCenter + Vector3.down * halfHeight;

        fallRotationAxis = Vector3.Cross(Vector3.up, longAxis).normalized;

        Vector3 directionToAirborne = (airborneCenter - groundedCenter).normalized;

        Vector3 testRotation = Vector3.Cross(fallRotationAxis, Vector3.down);
        float dotProduct = Vector3.Dot(testRotation, directionToAirborne);

        fallRotationDirection = dotProduct > 0 ? -1f : 1f;

        Debug.Log($"Fall Pivot calculado: {fallPivot}, Eje: {fallRotationAxis}, Dirección: {fallRotationDirection}");
    }

    void StartFalling()
    {
        isFalling = true;
        fallRotationAmount = 0f;
        fallTimer = 0f;
        if (fallSound != null)
            AudioSource.PlayClipAtPoint(fallSound, transform.position);
    }

    void RespawnPlayer()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("Respawn");

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
            transform.rotation = spawnPoint.transform.rotation;

            isFalling = false;
            fallRotationAmount = 0f;
            fallTimer = 0f;

            Debug.Log("Jugador respawneado tras caída");
        }
        else
        {
            Debug.LogError("No se encontró el punto de spawn para respawnear");
        }
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