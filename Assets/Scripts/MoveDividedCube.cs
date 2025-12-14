using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MoveDividedCube : MonoBehaviour
{
    public bool isActive = false;
    public MoveDividedCube otherHalf;

    public float rotSpeed = 300f;
    public float fallSpeed = 10f;
    public float inputCooldown = 0.1f;

    [Header("Referencias")]
    public AudioClip[] sounds;
    public AudioClip fallSound;

    [HideInInspector] public bool bMoving = false;
    [HideInInspector] public bool bFalling = false;

    private InputAction moveAction;
    private LayerMask layerMask;

    private Vector3 rotPoint, rotAxis;
    private float rotRemainder;
    private float rotDir;

    private bool canReceiveInput = true;
    private float cooldownTimer = 0f;

    private const float deadzone = 0.5f;

    void Start()
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

        layerMask = LayerMask.GetMask("Ground");
    }

    void OnDestroy()
    {
        moveAction.Disable();
        moveAction.Dispose();
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.0f, layerMask);
    }

    void Update()
    {
        if (transform.position.y < -5f)
        {
            HandleFallOffMap();
            return;
        }

        if (bFalling)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
            return;
        }

        if (bMoving)
        {
            float amount = rotSpeed * Time.deltaTime;
            if (amount >= rotRemainder)
            {
                transform.RotateAround(rotPoint, rotAxis, rotRemainder * rotDir);
                bMoving = false;

                Vector3 euler = transform.eulerAngles;
                euler.x = Mathf.Round(euler.x / 90f) * 90f;
                euler.y = Mathf.Round(euler.y / 90f) * 90f;
                euler.z = Mathf.Round(euler.z / 90f) * 90f;
                transform.rotation = Quaternion.Euler(euler);

                canReceiveInput = false;
                cooldownTimer = inputCooldown;
            }
            else
            {
                transform.RotateAround(rotPoint, rotAxis, amount * rotDir);
                rotRemainder -= amount;
            }
            return;
        }

        if (!canReceiveInput)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
                canReceiveInput = true;
            return;
        }

        if (!isActive)
            return;

        if (!IsGrounded())
        {
            bFalling = true;
            if (fallSound != null)
                AudioSource.PlayClipAtPoint(fallSound, transform.position, 1.5f);
            return;
        }

        Vector2 dir = moveAction.ReadValue<Vector2>();

        if (Mathf.Abs(dir.x) > deadzone || Mathf.Abs(dir.y) > deadzone)
        {
            HandleMovement(dir);
        }
    }

    void HandleFallOffMap()
    {
        if (DivisionManager.Instance != null)
            DivisionManager.Instance.ResetDivision();

        if (MoveCube.Instance != null)
        {
            MoveCube.Instance.HidePlayer();
            MoveCube.Instance.enabled = false;
        }

        LevelSequenceManager manager = FindObjectOfType<LevelSequenceManager>();
        if (manager != null)
            manager.RestartLevel();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void HandleMovement(Vector2 dir)
    {
        bMoving = true;
        canReceiveInput = false;

        if (sounds != null && sounds.Length > 0)
            AudioSource.PlayClipAtPoint(sounds[Random.Range(0, sounds.Length)], transform.position, 1.0f);

        Vector3 moveDir;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            moveDir = dir.x > 0 ? Vector3.right : Vector3.left;
        else
            moveDir = dir.y > 0 ? Vector3.forward : Vector3.back;

        CalculatePivot(moveDir);
    }

    void CalculatePivot(Vector3 direction)
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;

        float distToBottom = box.bounds.extents.y;
        float distToEdge = Mathf.Abs(direction.x) > 0 ? box.bounds.extents.x : box.bounds.extents.z;

        rotPoint = transform.position + direction * distToEdge + Vector3.down * distToBottom;
        rotAxis = Vector3.Cross(Vector3.up, direction);
        rotDir = 1f;
        rotRemainder = 90f;
    }
}
