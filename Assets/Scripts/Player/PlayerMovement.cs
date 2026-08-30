using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private CharacterController characterController;
    [SerializeField]
    private Transform visualRoot;
    [SerializeField]
    private Transform cameraTransform;

    [Header("Input")]
    [SerializeField]
    private InputActionReference moveAction;
    [SerializeField]
    private InputActionReference jumpAction;
    [SerializeField]
    private InputActionReference dashAction;

    [Header("Movement Mode")]
    [SerializeField]
    private PlayerMovementMode startingMovementMode = PlayerMovementMode.Normal;
    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 3f;
    [SerializeField]
    private float rotationSpeed = 12f;

    [Header("Jump")]
    [SerializeField]
    private float jumpHeight = 1.5f;
    [SerializeField]
    private float gravity = -20f;
    [SerializeField]
    private float groundedVelocity = -2f;
    [SerializeField]
    private float coyoteTime = 0.20f;
    [SerializeField]
    private float jumpBufferTime = 0.15f;

    [Header("Dash")]
    [SerializeField]
    private float dashSpeed = 6f;
    [SerializeField]
    private float dashDuration = 1f;
    [SerializeField]
    private float dashCooldown = 5f;

    [Header("Jump King")]
    [SerializeField]
    private Vector3 jumpKingHorizontalAxis = Vector3.forward;
    [SerializeField]
    private float jumpKingGroundSpeed = 2.5f;
    [SerializeField]
    private float jumpKingMaxChargeTime = 0.6f;
    [SerializeField]
    private float jumpKingHorizontalSpeed = 4.3f;
    [SerializeField]
    private float jumpKingMinVerticalSpeed = 4f;
    [SerializeField]
    private float jumpKingMaxVerticalSpeed = 11f;

    [Header("Dropper")]
    [SerializeField]
    private float dropperMoveSpeed = 4f;
    [SerializeField]
    private float dropperGravity = -12f;
    [SerializeField]
    private float dropperMaxFallSpeed = 25f;

    private float verticalVelocity;
    private bool controlsLocked;

    // Cannon
    private bool isLaunched;
    public bool IsLaunched => isLaunched;
    private Vector3 launchVelocity;

    // Fan
    private bool isInFan;
    private Vector3 fanVelocity;
    private Vector3 fanEntryVelocity;
    private float fanCaptureTimer;
    private float fanCaptureDuration;
    private bool fanHorizontalControlEnabled;
    private bool fanCaptureComplete;

    // Knockback
    private bool isKnockedBack;
    private Vector3 knockbackVelocity;

    // Hide
    private bool isHiding;
    public bool IsHiding => isHiding;

    // Strategy
    private PlayerMovementMode movementMode;
    private IPlayerMovementStrategy currentStrategy;
    private NormalMovementStrategy normalStrategy;
    private JumpKingMovementStrategy jumpKingStrategy;
    private DropperMovementStrategy dropperStrategy;

    public event Action<PlayerMovementMode> MovementModeChanged;

    public PlayerMovementMode MovementMode => movementMode;

    public float DashCooldownProgress => normalStrategy != null ? normalStrategy.DashCooldownProgress : 1f;
    public float JumpKingChargeRatio => jumpKingStrategy != null? jumpKingStrategy.ChargeRatio : 0f;

    public bool IsFanCaptureComplete => isInFan && fanCaptureComplete;
    public float FanVerticalVelocity => fanVelocity.y;

    // Strategies가 사용하는 공용 데이터.
    internal CharacterController Controller => characterController;
    internal Transform VisualRoot => visualRoot;
    internal Transform CameraTransform => cameraTransform;

    internal InputAction MoveInput => moveAction.action;
    internal InputAction JumpInput => jumpAction.action;
    internal InputAction DashInput => dashAction.action;

    internal float MoveSpeed => moveSpeed;
    internal float RotationSpeed => rotationSpeed;

    internal float JumpHeight => jumpHeight;
    internal float Gravity => gravity;
    internal float GroundedVelocity => groundedVelocity;
    internal float CoyoteTime => coyoteTime;
    internal float JumpBufferTime => jumpBufferTime;

    internal float DashSpeed => dashSpeed;
    internal float DashDuration => dashDuration;
    internal float DashCooldown => dashCooldown;

    internal float JumpKingGroundSpeed => jumpKingGroundSpeed;
    internal float JumpKingMaxChargeTime => jumpKingMaxChargeTime;
    internal float JumpKingHorizontalSpeed => jumpKingHorizontalSpeed;
    internal float JumpKingMinVerticalSpeed => jumpKingMinVerticalSpeed;
    internal float JumpKingMaxVerticalSpeed => jumpKingMaxVerticalSpeed;

    internal float DropperMoveSpeed => dropperMoveSpeed;
    internal float DropperGravity => dropperGravity;
    internal float DropperMaxFallSpeed => dropperMaxFallSpeed;

    internal float VerticalVelocity
    {
        get => verticalVelocity;
        set => verticalVelocity = value;
    }

    internal Vector3 JumpKingHorizontalAxis
    {
        get
        {
            Vector3 axis = Vector3.ProjectOnPlane(jumpKingHorizontalAxis, Vector3.up);

            if (axis.sqrMagnitude < 0.001f)
            {
                return Vector3.right;
            }

            return axis.normalized;
        }
    }

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        normalStrategy = new NormalMovementStrategy(this);
        jumpKingStrategy = new JumpKingMovementStrategy(this);
        dropperStrategy = new DropperMovementStrategy(this);

        movementMode = startingMovementMode;
        currentStrategy = GetStrategy(movementMode);
        currentStrategy.Enter();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        dashAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        dashAction.action.Disable();
    }

    private void Update()
    {
        normalStrategy.TickCooldown(Time.deltaTime);

        if (isHiding)
        {
            return;
        }

        if (isKnockedBack)
        {
            HandleKnockback();
            return;
        }

        if (isInFan)
        {
            HandleFanMovement();
            return;
        }

        if (isLaunched)
        {
            HandleLaunch();
            return;
        }

        if (controlsLocked)
        {
            return;
        }

        currentStrategy.Tick(Time.deltaTime);
    }

    public void SetMovementMode(PlayerMovementMode newMode)
    {
        if (movementMode == newMode)
        {
            return;
        }

        currentStrategy?.Exit();

        movementMode = newMode;
        currentStrategy = GetStrategy(movementMode);
        currentStrategy.Enter();

        MovementModeChanged?.Invoke(movementMode);
    }

    private IPlayerMovementStrategy GetStrategy(PlayerMovementMode mode)
    {
        switch (mode)
        {
            case PlayerMovementMode.JumpKing:
                return jumpKingStrategy;

            case PlayerMovementMode.Dropper:
                return dropperStrategy;

            default:
                return normalStrategy;
        }
    }

    private void CancelCurrentStrategyAction()
    {
        currentStrategy?.CancelActiveAction();
        normalStrategy?.CancelDash();
    }

    internal void RotateVisual(Vector3 horizontalVelocity, float deltaTime)
    {
        if (horizontalVelocity.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity.normalized, Vector3.up);
        visualRoot.rotation = Quaternion.Slerp(
            visualRoot.rotation,
            targetRotation,
            rotationSpeed * deltaTime
        );
    }

    // =========================
    // Cannon
    // =========================

    public void EnterCannon()
    {
        controlsLocked = true;
        CancelCurrentStrategyAction();
        visualRoot.gameObject.SetActive(false);
    }

    public void LaunchFromCannon(Vector3 position, Vector3 direction, float speed)
    {
        characterController.enabled = false;
        transform.position = position;
        characterController.enabled = true;

        visualRoot.gameObject.SetActive(true);
        controlsLocked = false;

        ResetFanState();

        isLaunched = true;
        launchVelocity = direction.normalized * speed;
    }

    private void HandleLaunch()
    {
        launchVelocity.y += gravity * Time.deltaTime;
        characterController.Move(launchVelocity * Time.deltaTime);

        if (characterController.isGrounded && launchVelocity.y <= 0f)
        {
            isLaunched = false;
            launchVelocity = Vector3.zero;
            verticalVelocity = groundedVelocity;
        }
    }

    // =========================
    // Fan
    // =========================

    public void EnterFan(float captureDuration)
    {
        if (isInFan)
        {
            return;
        }

        if (isLaunched)
        {
            fanEntryVelocity = launchVelocity;
        }
        else
        {
            fanEntryVelocity = new Vector3(0f, verticalVelocity, 0f);
        }

        isLaunched = false;
        launchVelocity = Vector3.zero;

        CancelCurrentStrategyAction();
        controlsLocked = true;

        isInFan = true;
        fanHorizontalControlEnabled = false;
        fanVelocity = fanEntryVelocity;
        fanCaptureTimer = 0f;
        fanCaptureDuration = Mathf.Max(captureDuration, 0.001f);
        fanCaptureComplete = false;
    }

    private void HandleFanMovement()
    {
        if (!fanCaptureComplete)
        {
            fanCaptureTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fanCaptureTimer / fanCaptureDuration);
            fanVelocity = Vector3.Lerp(fanEntryVelocity, Vector3.zero,t);

            if (t >= 1f)
            {
                fanVelocity = Vector3.zero;
                fanCaptureComplete = true;
            }

            characterController.Move(fanVelocity * Time.deltaTime);
            return;
        }

        if (!fanHorizontalControlEnabled)
        {
            fanVelocity.x = 0f;
            fanVelocity.z = 0f;

            characterController.Move(fanVelocity * Time.deltaTime);
            return;
        }

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 moveDirection = cameraForward * input.y + cameraRight * input.x;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        fanVelocity.x = moveDirection.x * moveSpeed;
        fanVelocity.z = moveDirection.z * moveSpeed;

        RotateVisual(new Vector3(fanVelocity.x, 0f, fanVelocity.z), Time.deltaTime);
        characterController.Move(fanVelocity * Time.deltaTime);
    }

    public void AddFanVerticalAcceleration(float acceleration)
    {
        if (!isInFan || !fanCaptureComplete)
        {
            return;
        }

        fanVelocity.y += acceleration * Time.deltaTime;
    }

    public void SetFanVerticalVelocity(float velocity)
    {
        if (!isInFan)
        {
            return;
        }

        fanVelocity.y = velocity;
    }

    public void EnableFanHorizontalControl()
    {
        fanHorizontalControlEnabled = true;
    }

    public void ExitFan()
    {
        if (!isInFan)
        {
            return;
        }

        verticalVelocity = fanVelocity.y;
        ResetFanState();
        controlsLocked = false;
    }

    private void ResetFanState()
    {
        isInFan = false;
        fanVelocity = Vector3.zero;
        fanEntryVelocity = Vector3.zero;
        fanCaptureTimer = 0f;
        fanCaptureDuration = 0f;
        fanCaptureComplete = false;
        fanHorizontalControlEnabled = false;
    }

    // =========================
    // Respawn
    // =========================

    public void ResetForRespawn()
    {
        verticalVelocity = groundedVelocity;

        normalStrategy.Reset();
        jumpKingStrategy.Reset();
        dropperStrategy.Reset();

        controlsLocked = false;

        isLaunched = false;
        launchVelocity = Vector3.zero;

        isKnockedBack = false;
        knockbackVelocity = Vector3.zero;

        isHiding = false;

        ResetFanState();

        visualRoot.gameObject.SetActive(true);
    }

    // =========================
    // Knockback
    // =========================

    private void HandleKnockback()
    {
        knockbackVelocity.y += gravity * Time.deltaTime;

        characterController.Move(knockbackVelocity * Time.deltaTime);

        if (characterController.isGrounded && knockbackVelocity.y <= 0f)
        {
            isKnockedBack = false;
            knockbackVelocity = Vector3.zero;
            verticalVelocity = groundedVelocity;
        }
    }

    public void ApplyKnockback(Vector3 objectVelocity, float forceMultiplier, float upwardSpeed)
    {
        isLaunched = false;
        launchVelocity = Vector3.zero;

        ResetFanState();
        CancelCurrentStrategyAction();

        controlsLocked = false;
        visualRoot.gameObject.SetActive(true);

        knockbackVelocity = objectVelocity * forceMultiplier;
        knockbackVelocity.y = Mathf.Max(knockbackVelocity.y,upwardSpeed);

        isKnockedBack = true;
    }

    // =========================
    // Hide
    // =========================

    public void EnterHide()
    {
        if (isHiding)
        {
            return;
        }

        CancelCurrentStrategyAction();

        isLaunched = false;
        launchVelocity = Vector3.zero;

        isKnockedBack = false;
        knockbackVelocity = Vector3.zero;

        ResetFanState();

        verticalVelocity = 0f;

        controlsLocked = true;
        isHiding = true;

        visualRoot.gameObject.SetActive(false);
    }

    public void ExitHide(Vector3 position)
    {
        if (!isHiding)
        {
            return;
        }

        characterController.enabled = false;
        transform.position = position;
        characterController.enabled = true;

        visualRoot.gameObject.SetActive(true);

        verticalVelocity = groundedVelocity;

        isHiding = false;
        controlsLocked = false;
    }

    public void SetControlsLocked(bool locked)
    {
        controlsLocked = locked;
    }
}
