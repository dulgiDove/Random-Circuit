using Unity.Netcode;
using UnityEngine;

public class FanModelRotator : MonoBehaviour
{
    [Header("Rotate")]
    [SerializeField]
    private Vector3 rotationAxis = Vector3.up;
    [SerializeField]
    private float rotationSpeed = 360f;

    private MapActivity mapActivity;
    private Quaternion baseRotation;

    private void Awake()
    {
        mapActivity = GetComponentInParent<MapActivity>();
        Debug.Assert(mapActivity != null);

        baseRotation = transform.localRotation;
    }

    private void Update()
    {
        if (!mapActivity.IsActive)
            return;

        double serverTime = NetworkManager.Singleton.ServerTime.Time;
        double elapsed = serverTime - mapActivity.ActivatedServerTime;

        float angle = Mathf.Repeat((float)(elapsed * rotationSpeed), 360f);

        transform.localRotation = baseRotation * Quaternion.AngleAxis(angle, rotationAxis.normalized);
    }
}