using UnityEngine;

public class NormalMovementStrategy : IPlayerMovementStrategy
{
    private readonly PlayerMovement owner;

    private float coyoteTimer;
    private float jumpBufferTimer;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector3 dashDirection;

    public float DashCooldownProgress
    {
        get
        {
            if (owner.DashCooldown <= 0f)
            {
                return 1f;
            }

            return 1f - Mathf.Clamp01(dashCooldownTimer / owner.DashCooldown);
        }
    }

    public NormalMovementStrategy(PlayerMovement owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        CancelActiveAction();
    }

    public void Exit()
    {
        CancelActiveAction();
    }

    public void Tick(float deltaTime)
    {
        Vector2 input = owner.MoveInput.ReadValue<Vector2>();

        Vector3 cameraForward = owner.CameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = owner.CameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 moveDirection =
            cameraForward * input.y +
            cameraRight * input.x;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        bool grounded = owner.Controller.isGrounded;

        // Coyote Time
        if (grounded)
        {
            coyoteTimer = owner.CoyoteTime;
        }
        else
        {
            coyoteTimer -= deltaTime;
        }

        // Jump Buffer
        if (owner.JumpInput.WasPressedThisFrame())
        {
            jumpBufferTimer = owner.JumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= deltaTime;
        }

        if (grounded && owner.VerticalVelocity < 0f)
        {
            owner.VerticalVelocity = owner.GroundedVelocity;
        }

        bool canJump = coyoteTimer > 0f && jumpBufferTimer > 0f;

        if (canJump)
        {
            owner.VerticalVelocity = Mathf.Sqrt(owner.JumpHeight * -2f * owner.Gravity);
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
        }

        if (!grounded)
        {
            owner.VerticalVelocity += owner.Gravity * deltaTime;
        }

        if (!isDashing && grounded && owner.VerticalVelocity <= 0f && dashCooldownTimer <= 0f && owner.DashInput.WasPressedThisFrame())
        {
            StartDash(moveDirection);
        }

        Vector3 horizontalVelocity;

        if (isDashing)
        {
            dashTimer -= deltaTime;

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                dashDirection = moveDirection.normalized;
            }

            horizontalVelocity = dashDirection * owner.DashSpeed;

            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
        else
        {
            horizontalVelocity = moveDirection * owner.MoveSpeed;
        }

        owner.RotateVisual(horizontalVelocity, deltaTime);

        Vector3 velocity = horizontalVelocity + Vector3.up * owner.VerticalVelocity;
        owner.Controller.Move(velocity * deltaTime);
    }

    public void TickCooldown(float deltaTime)
    {
        if (dashCooldownTimer <= 0f)
        {
            return;
        }

        dashCooldownTimer -= deltaTime;
        dashCooldownTimer = Mathf.Max(dashCooldownTimer, 0f);
    }

    public void CancelActiveAction()
    {
        CancelDash();
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
    }

    public void Reset()
    {
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;

        isDashing = false;
        dashTimer = 0f;
        dashCooldownTimer = 0f;
        dashDirection = Vector3.zero;
    }

    private void StartDash(Vector3 moveDirection)
    {
        isDashing = true;
        dashTimer = owner.DashDuration;
        dashCooldownTimer = owner.DashCooldown;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            dashDirection = moveDirection.normalized;
        }
        else
        {
            dashDirection = Vector3.ProjectOnPlane(owner.VisualRoot.forward, Vector3.up).normalized;

            if (dashDirection.sqrMagnitude < 0.001f)
            {
                dashDirection = Vector3.forward;
            }
        }
    }

    public void CancelDash()
    {
        isDashing = false;
        dashTimer = 0f;
        dashDirection = Vector3.zero;
    }
}
