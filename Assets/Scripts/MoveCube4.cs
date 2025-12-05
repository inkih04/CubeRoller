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

    [Header("Configuración Bloxorz")]
    public float rotSpeed = 300f;
    public float fallSpeed = 20f;

    [Header("Referencias")]
    public GameObject ghostPlayer;
    public AudioClip[] sounds;
    public AudioClip fallSound;
    public TMP_Text movesText;

    // Variables de estado
    private bool isMoving = false;
    private bool isFalling = false;
    private bool isVictory = false; // <--- NUEVA VARIABLE IMPORTANTE
    private int moveCount = 0;

    // Variables de cálculo
    private Vector3 pivot;
    private Vector3 rotAxis;
    private float degreesToRotate = 90f;
    private float currentRotated = 0f;
    private float rotationDirection = 0f;
    private float targetX, targetZ;
    private bool inputProcessed = true;
    private bool controlsActive = true;

    InputAction moveAction;
    LayerMask groundLayerMask;
    BoxCollider boxCollider;
    Rigidbody rb;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); SceneManager.sceneLoaded += OnSceneLoaded; }
        else { Destroy(gameObject); return; }

        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void OnDestroy() { if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu") return;

        transform.position = Vector3.up * 50f; // Zona segura al cargar
        this.enabled = true;

        // Resetear estados
        isVictory = false; // <--- RESETEAR AQUÍ
        isMoving = false;
        isFalling = false;

        HidePlayer();

        GameObject counterGO = GameObject.FindGameObjectWithTag("MoveCounter");
        if (counterGO != null && counterGO.TryGetComponent(out TMP_Text txt)) { movesText = txt; movesText.text = "Moves: " + moveCount; }
    }

    void Start()
    {
        if (ghostPlayer != null)
        {
            Renderer[] renderers = ghostPlayer.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers) r.enabled = false;
        }

        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null) moveAction = playerInput.actions["Move"];
        else
        {
            moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
            moveAction.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            moveAction.Enable();
        }

        groundLayerMask = LayerMask.GetMask("Ground", "Default");
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

    // --- LÓGICA DE DETECCIÓN DE ESTADO ---

    // Método para saber si el cubo está QUIETO (importante para el WinTile)
    public bool IsStopped()
    {
        return !isMoving && !isFalling && rb.linearVelocity.sqrMagnitude < 0.01f;
    }

    public bool IsVertical()
    {
        float tolerance = 0.1f;
        // Compara la altura Y con X y Z. Si es mayor que ambos, está de pie.
        return boxCollider.bounds.size.y > (boxCollider.bounds.size.x + tolerance) &&
               boxCollider.bounds.size.y > (boxCollider.bounds.size.z + tolerance);
    }

    public void FallIntoHole(string nextLevel)
    {
        if (isFalling || isVictory) return;
        StartCoroutine(FallWinSequence(nextLevel));
    }

    IEnumerator FallWinSequence(string nextLevel)
    {
        isVictory = true; // <--- MARCAMOS VICTORIA PARA EVITAR MUERTE
        isFalling = true;
        SetPlayerControl(false);

        if (fallSound != null) AudioSource.PlayClipAtPoint(fallSound, transform.position);

        // Desactivar collider para atravesar el suelo
        boxCollider.enabled = false;

        float dropDistance = 5f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.down * dropDistance;
        float t = 0;

        // Caída suave
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // Llamar al Manager
        LevelSequenceManager manager = FindObjectOfType<LevelSequenceManager>();
        if (manager != null) manager.LoadNextLevel(nextLevel);
        else SceneManager.LoadScene(nextLevel);

        // Restaurar collider (importante para el siguiente nivel)
        boxCollider.enabled = true;
    }

    // ... WaitForLanding, SetPlayerControl, etc. ...

    IEnumerator WaitForLanding()
    {
        yield return new WaitForSeconds(0.1f);
        float distToGround = boxCollider.bounds.extents.y + 0.1f;
        while (!Physics.Raycast(transform.position, Vector3.down, distToGround, groundLayerMask)) yield return null;

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

    public void SetPlayerControl(bool active) { controlsActive = active; if (!active) isMoving = false; }

    void Update()
    {
        // --- CORRECCIÓN CRÍTICA: NO MORIR SI ES VICTORIA ---
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
        // --------------------------------------------------

        if (!rb.isKinematic && !isFalling) return;

        // Si estamos cayendo por victoria (y collider apagado) no usamos Translate manual, la corrutina lo hace.
        // Si estamos cayendo por muerte (isFalling y NO isVictory), usamos esto:
        if (isFalling && !isVictory)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
            return;
        }

        if (isMoving) { PerformRotation(); return; }
        if (controlsActive) HandleInput();
    }

    // ... HandleInput, SimulateAndMove, etc. (El resto es igual) ...
    void HandleInput()
    {
        if (!isGrounded()) { StartFalling(); return; }
        if (moveAction == null) return;
        Vector2 input = moveAction.ReadValue<Vector2>();
        bool hasInput = input.magnitude > 0.5f;
        if (!hasInput) { inputProcessed = false; return; }
        if (inputProcessed) return;
        inputProcessed = true;
        Vector3 direction = Vector3.zero;
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y)) direction = input.x > 0 ? Vector3.right : Vector3.left;
        else direction = input.y > 0 ? Vector3.forward : Vector3.back;
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
        else CalculatePivot(transform, direction);
        StartRealRotation(direction);
    }
    void StartRealRotation(Vector3 dir)
    {
        isMoving = true; currentRotated = 0f; degreesToRotate = 90f;
        if (sounds != null && sounds.Length > 0) AudioSource.PlayClipAtPoint(sounds[UnityEngine.Random.Range(0, sounds.Length)], transform.position);
    }
    bool CalculatePivot(Transform targetTransform, Vector3 dir)
    {
        if (boxCollider == null) return false;
        float distToBottom = boxCollider.bounds.extents.y;
        float distToEdge = (Mathf.Abs(dir.x) > 0) ? boxCollider.bounds.extents.x : boxCollider.bounds.extents.z;
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
            if (ghostPlayer != null) { transform.position = ghostPlayer.transform.position; transform.rotation = ghostPlayer.transform.rotation; }
            else
            {
                Vector3 euler = transform.eulerAngles;
                euler.x = Mathf.Round(euler.x / 90) * 90; euler.y = Mathf.Round(euler.y / 90) * 90; euler.z = Mathf.Round(euler.z / 90) * 90;
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
        if (!rb.isKinematic) return true;
        if (isMoving) return true;
        float dist = boxCollider.bounds.extents.y + 0.1f;
        return Physics.Raycast(transform.position, Vector3.down, dist, groundLayerMask, QueryTriggerInteraction.Collide);
    }
    void StartFalling()
    {
        isFalling = true;
        if (fallSound != null) AudioSource.PlayClipAtPoint(fallSound, transform.position);
    }
}