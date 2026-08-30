using UnityEngine;

public class FanModelRotator : MonoBehaviour
{
    [Header("Rotate")]
    [SerializeField]
    private Vector3 rotationAxis = Vector3.up;
    [SerializeField]
    private float rotationSpeed = 360f;

    private void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self);
    }
}