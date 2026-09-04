using UnityEngine;

public class JumpKingMovementStrategy : IPlayerMovementStrategy
{
    private readonly PlayerMovement owner;

    private bool isCharging;
    private float chargeTime;
    private float jumpHorizontalVelocity;

    public float ChargeRatio
    {
        get
        {
            if (owner.JumpKingMaxChargeTime <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(chargeTime / owner.JumpKingMaxChargeTime);
        }
    }

    public JumpKingMovementStrategy(PlayerMovement owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        Reset();
        owner.VerticalVelocity = owner.GroundedVelocity;
    }

    public void Exit()
    {
        Reset();
    }

    public void Tick(float deltaTime)
    {
        Vector2 input = owner.MoveInput.ReadValue<Vector2>();

        float horizontalInput = input.x;

        if (Mathf.Abs(horizontalInput) < 0.1f)
        {
            horizontalInput = 0f;
        }

        Vector3 horizontalAxis = owner.JumpKingHorizontalAxis;
        bool grounded = owner.Controller.isGrounded;
        bool launchedThisFrame = false;

        if (grounded)
        {
            if (owner.VerticalVelocity < 0f)
            {
                owner.VerticalVelocity = owner.GroundedVelocity;
                jumpHorizontalVelocity = 0f;
            }

            if (!isCharging && owner.JumpInput.WasPressedThisFrame())
            {
                isCharging = true;
                chargeTime = 0f;
            }

            if (isCharging && owner.JumpInput.IsPressed())
            {
                chargeTime += deltaTime;
                chargeTime = Mathf.Min(chargeTime, owner.JumpKingMaxChargeTime);
            }

            if (isCharging && owner.JumpInput.WasReleasedThisFrame())
            {
                float charge = ChargeRatio;
                float horizontalSpeed = owner.JumpKingHorizontalSpeed;
                float verticalSpeed = Mathf.Lerp(owner.JumpKingMinVerticalSpeed, owner.JumpKingMaxVerticalSpeed, charge);

                Vector3 facingDirection = Vector3.ProjectOnPlane(owner.VisualRoot.forward, Vector3.up).normalized;

                float direction = Mathf.Sign(Vector3.Dot(facingDirection, horizontalAxis));

                jumpHorizontalVelocity = horizontalSpeed * direction;
                owner.VerticalVelocity = verticalSpeed;
                isCharging = false;
                chargeTime = 0f;
                launchedThisFrame = true;
            }
        }
        else
        {
            owner.VerticalVelocity += owner.Gravity * deltaTime;
        }

        float horizontalSpeedThisFrame;

        if (!grounded || launchedThisFrame)
        {
            horizontalSpeedThisFrame = jumpHorizontalVelocity;
        }
        else if (isCharging)
        {
            horizontalSpeedThisFrame = 0f;
        }
        else
        {
            horizontalSpeedThisFrame = horizontalInput * owner.JumpKingGroundSpeed;
        }

        Vector3 horizontalVelocity = horizontalAxis * horizontalSpeedThisFrame;

        if (Mathf.Abs(horizontalSpeedThisFrame) > 0.001f)
        {
            owner.VisualRoot.rotation = Quaternion.LookRotation(horizontalVelocity.normalized, Vector3.up);
        }

        Vector3 velocity = horizontalVelocity + Vector3.up * owner.VerticalVelocity;
        CollisionFlags collisionFlags =owner.Controller.Move(velocity * deltaTime);

        // 머리 충돌
        if ((collisionFlags & CollisionFlags.Above) != 0 && owner.VerticalVelocity > 0f)
        {
            owner.VerticalVelocity = 0f;
        }

        // 벽 충돌
        if ((collisionFlags & CollisionFlags.Sides) != 0 && Mathf.Abs(jumpHorizontalVelocity) > 0.001f)
        {
            jumpHorizontalVelocity = -jumpHorizontalVelocity;
            Vector3 bounceDirection = horizontalAxis * jumpHorizontalVelocity;
            owner.VisualRoot.rotation = Quaternion.LookRotation(bounceDirection.normalized, Vector3.up);
        }
    }

    public void CancelActiveAction()
    {
        Reset();
    }

    public void Reset()
    {
        isCharging = false;
        chargeTime = 0f;
        jumpHorizontalVelocity = 0f;
    }
}
