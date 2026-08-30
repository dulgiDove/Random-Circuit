using UnityEngine;

public class Cannon : Interactable
{
    private enum CannonState
    {
        Idle,
        Occupied,
        Returning
    }


    [Header("Cannon Rotation")]
    [SerializeField]
    private Transform barrelPivot;
    [SerializeField]
    private float minAngle = -25f;
    [SerializeField]
    private float maxAngle = 65f;
    [SerializeField]
    private float stopAngle = 0f;
    [SerializeField]
    private float rotationSpeed = 45f;


    [Header("Launch")]
    [SerializeField]
    private Transform launchPoint;
    [SerializeField]
    private float fullChargeTime = 4f;
    [SerializeField]
    private float maxLaunchSpeed = 30f;


    private Quaternion baseRotation;

    private float currentAngle;
    private float direction = 1f;

    private float chargeTime;

    private CannonState state = CannonState.Idle;

    public float ChargeRatio
    {
        get
        {
            if (fullChargeTime <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(chargeTime / fullChargeTime);
        }
    }


    private void Start()
    {
        baseRotation = barrelPivot.localRotation;

        currentAngle = stopAngle;
        direction = 1f;

        SetBarrelRotation();
    }


    private void Update()
    {
        if (state == CannonState.Idle)
        {
            return;
        }

        RotateBarrel();
    }


    private void RotateBarrel()
    {
        float previousAngle = currentAngle;


        currentAngle += rotationSpeed * direction * Time.deltaTime;
        
        if (currentAngle >= maxAngle)
        {
            currentAngle = maxAngle;
            direction = -1f;
        }
        else if (currentAngle <= minAngle)
        {
            currentAngle = minAngle;
            direction = 1f;
        }

        if (state == CannonState.Returning)
        {
            bool crossedStopAngle =(previousAngle < stopAngle && currentAngle >= stopAngle) || (previousAngle > stopAngle && currentAngle <= stopAngle);
            if (crossedStopAngle)
            {
                StopCannon();
                return;
            }
        }

        SetBarrelRotation();
    }

    private void SetBarrelRotation()
    {
        barrelPivot.localRotation = baseRotation * Quaternion.Euler(0f, currentAngle, 0f);
    }

    private void StopCannon()
    {
        state = CannonState.Idle;

        currentAngle = stopAngle;
        direction = 1f;
        chargeTime = 0f;

        SetBarrelRotation();
    }

    public override bool CanInteract(PlayerInteraction player)
    {
        return state == CannonState.Idle;
    }


    public override void InteractStart(PlayerInteraction player)
    {
        state = CannonState.Occupied;
        chargeTime = 0f;

        PlayerMovement movement =player.GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.EnterCannon();
        }
    }


    public override void InteractHold(PlayerInteraction player)
    {
        if (state != CannonState.Occupied)
        {
            return;
        }

        chargeTime += Time.deltaTime;
        chargeTime = Mathf.Min(chargeTime, fullChargeTime);
    }


    public override void InteractEnd(PlayerInteraction player)
    {
        if (state != CannonState.Occupied)
        {
            return;
        }

        FirePlayer(player);
        state = CannonState.Returning;
    }


    private void FirePlayer(PlayerInteraction player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (movement == null)
        {
            return;
        }

        float launchSpeed = maxLaunchSpeed * ChargeRatio;
        movement.LaunchFromCannon(launchPoint.position, launchPoint.forward, launchSpeed);
    }
}