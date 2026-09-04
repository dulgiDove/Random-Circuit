using Unity.Netcode;
using UnityEngine;

public class RotatingBar : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 90f;
    private Quaternion baseRotation;

    private Rigidbody rb;

    private MapActivity mapActivity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mapActivity = GetComponentInParent<MapActivity>();

        Debug.Assert(rb != null);
        Debug.Assert(mapActivity != null);

        baseRotation = rb.rotation;
    }


    private void FixedUpdate()
    {
        if (!mapActivity.IsActive)
            return;

        double serverTime = NetworkManager.Singleton.ServerTime.Time;
        double elapsed =serverTime - mapActivity.ActivatedServerTime;
        float angle =Mathf.Repeat((float)(elapsed * rotationSpeed), 360f);

        Quaternion targetRotation = baseRotation * Quaternion.Euler(0f, angle, 0f);

        rb.MoveRotation(targetRotation);
    }
}