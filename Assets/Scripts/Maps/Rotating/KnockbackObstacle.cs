using UnityEngine;

public class KnockbackObstacle : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField]
    private Transform motionRoot;

    [Header("Knockback")]
    [SerializeField]
    private float forceMultiplier = 1.5f;
    [SerializeField]
    private float upwardSpeed = 4f;

    private Vector3 previousPosition;
    private Quaternion previousRotation;

    private Vector3 linearVelocity;
    private Vector3 angularVelocity;

    private void Start()
    {
        if (motionRoot == null)
        {
            motionRoot = transform;
        }

        previousPosition = motionRoot.position;
        previousRotation = motionRoot.rotation;
    }


    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        linearVelocity = (motionRoot.position - previousPosition) / deltaTime;
        Quaternion rotationDelta = motionRoot.rotation * Quaternion.Inverse(previousRotation);
        rotationDelta.ToAngleAxis(out float angle,out Vector3 axis);

        if (angle > 180f)
        {
            angle -= 360f;
        }

        angularVelocity = axis * angle * Mathf.Deg2Rad / deltaTime;
        previousPosition = motionRoot.position;
        previousRotation = motionRoot.rotation;
    }


    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player == null)
        {
            return;
        }

        if (player.IsHiding)
        {
            return;
        }

        Vector3 playerPosition = player.transform.position;
        Vector3 offset = playerPosition - motionRoot.position;
        Vector3 rotationalVelocity = Vector3.Cross( angularVelocity, offset);
        Vector3 objectVelocity = linearVelocity + rotationalVelocity;
        player.ApplyKnockback(objectVelocity, forceMultiplier, upwardSpeed);
    }
}