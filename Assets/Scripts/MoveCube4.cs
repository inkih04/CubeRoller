using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MoveCube : MonoBehaviour
{
    InputAction moveAction;

    bool bMoving = false;
    bool bFalling = false;

    public float rotSpeed;
    public float fallSpeed;

    Vector3 rotPoint, rotAxis;
    float rotRemainder;
    float rotDir;
    LayerMask layerMask;

    public AudioClip[] sounds;
    public AudioClip fallSound;
    public TMP_Text movesText;
    private int moveCount = 0;


    public enum Orientation
    {
        VerticalY,
        HorizontalX,
        HorizontalZ
    }

    private Orientation currentOrientation = Orientation.HorizontalX;

    [Header("Debug Orientación")]
    public bool showOrientationDebug = true;


    public bool IsVertical()
    {
        return currentOrientation == Orientation.VerticalY;
    }

    public Orientation GetOrientation()
    {
        return currentOrientation;
    }

    void DetectOrientation()
    {
        Vector3 up = transform.up;

        float dotY = Mathf.Abs(Vector3.Dot(up, Vector3.up));
        float dotX = Mathf.Abs(Vector3.Dot(up, Vector3.right));
        float dotZ = Mathf.Abs(Vector3.Dot(up, Vector3.forward));

        if (dotY > dotX && dotY > dotZ)
        {
            currentOrientation = Orientation.VerticalY;
        }
        else if (dotX > dotZ)
        {
            currentOrientation = Orientation.HorizontalX;
        }
        else
        {
            currentOrientation = Orientation.HorizontalZ;
        }

        // Debug visual
        if (showOrientationDebug)
        {
            string orientText = currentOrientation == Orientation.VerticalY ? "VERTICAL ?" : "HORIZONTAL";
            Debug.Log("Orientación: " + orientText);
        }
    }

    bool isGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.0f, layerMask))
            return true;

        return false;
    }

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        layerMask = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        if (!bMoving && !bFalling)
        {
            DetectOrientation();
        }

        if (bFalling)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
        }
        else if (bMoving)
        {
            float amount = rotSpeed * Time.deltaTime;
            if (amount > rotRemainder)
            {
                transform.RotateAround(rotPoint, rotAxis, rotRemainder * rotDir);
                bMoving = false;
                moveCount++;
                if (movesText != null)
                    movesText.text = "Moves: " + moveCount.ToString();

                DetectOrientation();
            }
            else
            {
                transform.RotateAround(rotPoint, rotAxis, amount * rotDir);
                rotRemainder -= amount;
            }
        }
        else
        {
            if (!isGrounded())
            {
                bFalling = true;
                AudioSource.PlayClipAtPoint(fallSound, transform.position, 1.5f);
            }

            Vector2 dir = moveAction.ReadValue<Vector2>();
            if (Math.Abs(dir.x) > 0.99 || Math.Abs(dir.y) > 0.99)
            {
                bMoving = true;

                int iSound = UnityEngine.Random.Range(0, sounds.Length);
                AudioSource.PlayClipAtPoint(sounds[iSound], transform.position, 1.0f);

                if (dir.x > 0.99)
                {
                    rotDir = -1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = new Vector3(0.0f, 0.0f, 1.0f);
                    rotPoint = transform.position + new Vector3(0.5f, -0.5f, 0.0f);
                }
                else if (dir.x < -0.99)
                {
                    rotDir = 1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = new Vector3(0.0f, 0.0f, 1.0f);
                    rotPoint = transform.position + new Vector3(-0.5f, -0.5f, 0.0f);
                }
                else if (dir.y > 0.99)
                {
                    rotDir = 1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = new Vector3(1.0f, 0.0f, 0.0f);
                    rotPoint = transform.position + new Vector3(0.0f, -0.5f, 0.5f);
                }
                else if (dir.y < -0.99)
                {
                    rotDir = -1.0f;
                    rotRemainder = 90.0f;
                    rotAxis = new Vector3(1.0f, 0.0f, 0.0f);
                    rotPoint = transform.position + new Vector3(0.0f, -0.5f, -0.5f);
                }
            }
        }
    }
}


