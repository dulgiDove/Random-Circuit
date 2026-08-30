using UnityEngine;

public class DropperMovementStrategy : IPlayerMovementStrategy
{
    private readonly PlayerMovement owner;

    public DropperMovementStrategy(PlayerMovement owner)
    {
        this.owner = owner;
    }

    public void Enter()
    {
        owner.VerticalVelocity = owner.GroundedVelocity;
    }

    public void Exit()
    {
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

        Vector3 moveDirection = cameraForward * input.y + cameraRight * input.x;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        if (owner.Controller.isGrounded && owner.VerticalVelocity < 0f)
        {
            owner.VerticalVelocity = owner.GroundedVelocity;
        }
        else
        {
            owner.VerticalVelocity += owner.DropperGravity * deltaTime;

            owner.VerticalVelocity = Mathf.Max(owner.VerticalVelocity, -owner.DropperMaxFallSpeed);
        }

        Vector3 horizontalVelocity = moveDirection * owner.DropperMoveSpeed;
        Vector3 velocity = horizontalVelocity + Vector3.up * owner.VerticalVelocity;
        owner.Controller.Move(velocity * deltaTime);
        owner.RotateVisual(horizontalVelocity, deltaTime);
    }

    public void CancelActiveAction()
    {
    }

    public void Reset()
    {
    }
}
