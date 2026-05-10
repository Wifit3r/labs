using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Контролер головного героя: постійний рух вперед, зміщення вліво-вправо,
/// стрибок через перешкоди, прискорення з обмеженням по часу.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Рух вперед")]
    public float forwardSpeed = 10f;

    [Header("Рух вліво-вправо")]
    public float laneDistance = 3f;
    public float sidewaysSpeed = 15f;

    [Header("Стрибок")]
    public float jumpForce = 8f;
    public float gravity = -20f;

    [Header("Прискорення")]
    public float boostMultiplier = 2f;
    public float maxBoostDuration = 3f;

    private float currentBoostTime;
    private bool isBoosting;

    private int currentLane; // -1 = ліва, 0 = центр, 1 = права
    private float verticalVelocity;
    private bool isGrounded = true;

    private Vector3 startPosition;
    private bool isAlive = true;

    private CharacterController controller;
    private GameManager gameManager;

    // Input System
    private Keyboard keyboard;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        gameManager = FindObjectOfType<GameManager>();
        startPosition = transform.position;
        currentLane = 0;
        currentBoostTime = 0f;
        keyboard = Keyboard.current;
    }

    void Update()
    {
        if (!isAlive) return;
        if (gameManager != null && gameManager.IsFinished) return;
        if (keyboard == null) keyboard = Keyboard.current;
        if (keyboard == null) return;

        HandleInput();
        Move();
    }

    void HandleInput()
    {
        // Рух вліво
        if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
        {
            if (currentLane > -1)
                currentLane--;
        }

        // Рух вправо
        if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
        {
            if (currentLane < 1)
                currentLane++;
        }

        // Стрибок
        if ((keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame) && isGrounded)
        {
            verticalVelocity = jumpForce;
            isGrounded = false;
        }

        // Прискорення (Shift)
        if (keyboard.leftShiftKey.isPressed && currentBoostTime < maxBoostDuration)
        {
            isBoosting = true;
            currentBoostTime += Time.deltaTime;
            if (currentBoostTime >= maxBoostDuration)
            {
                isBoosting = false;
            }
        }
        else
        {
            isBoosting = false;
        }
    }

    void Move()
    {
        float speed = forwardSpeed * (isBoosting ? boostMultiplier : 1f);

        // Рух вперед (вздовж осі Z)
        Vector3 moveDirection = Vector3.forward * speed;

        // Цільова позиція по X (полоси)
        float targetX = currentLane * laneDistance;
        float deltaX = targetX - transform.position.x;
        moveDirection.x = deltaX * sidewaysSpeed;

        // Гравітація та стрибок
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        else
        {
            verticalVelocity = -1f; // тримає на землі
        }
        moveDirection.y = verticalVelocity;

        controller.Move(moveDirection * Time.deltaTime);

        // Перевірка землі
        isGrounded = controller.isGrounded;
    }

    /// <summary>
    /// Перемістити героя на старт (при падінні в яму).
    /// </summary>
    public void ResetToStart()
    {
        controller.enabled = false;
        transform.position = startPosition;
        controller.enabled = true;
        currentLane = 0;
        verticalVelocity = 0f;
        isGrounded = true;
        currentBoostTime = 0f;
        isBoosting = false;
    }

    public void Die()
    {
        isAlive = false;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Obstacle"))
        {
            if (hit.normal.z < -0.5f || Mathf.Abs(hit.normal.x) > 0.5f)
            {
                GameData.Instance.LoseLife();
                if (GameData.Instance.IsGameOver)
                    Die();
                else
                    ResetToStart();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pit"))
        {
            GameData.Instance.LoseLife();
            if (!GameData.Instance.IsGameOver)
                ResetToStart();
            else
                Die();
        }
        else if (other.CompareTag("Finish"))
        {
            GameData.Instance.FinishLevel();
            if (gameManager != null)
                gameManager.FinishGame();
        }
        else if (other.CompareTag("BoostPickup"))
        {
            currentBoostTime = 0f;
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Coin"))
        {
            GameData.Instance.CollectCoin();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Trap"))
        {
            GameData.Instance.LoseLife();
            if (GameData.Instance.IsGameOver)
                Die();
            else
                ResetToStart();
        }
    }

    public float GetBoostRemaining()
    {
        return Mathf.Max(0f, maxBoostDuration - currentBoostTime);
    }

    public bool IsBoosting => isBoosting;
}
