using UnityEngine;

public class RotatingBar : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 90f;

    private Rigidbody rb;

    private MapActivity mapActivity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mapActivity = GetComponentInParent<MapActivity>();
    }


    private void FixedUpdate()
    {
        if (mapActivity != null && !mapActivity.IsActive)
        {
            return;
        }

        Quaternion deltaRotation = Quaternion.Euler(0f, rotationSpeed * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}