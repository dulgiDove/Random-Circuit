using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Cinemachine;

public class CameraSwipeOrbit : MonoBehaviour, IDragHandler
{
    [Header("Cinemachine")]
    [SerializeField]
    private CinemachineOrbitalFollow orbitalFollow;

    [Header("Swipe Sensitivity")]
    [SerializeField]
    private float horizontalDegreesPerScreen = 180f;
    [SerializeField]
    private float verticalDegreesPerScreen = 120f;

    private void Awake()
    {
        Debug.Assert(orbitalFollow != null);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.delta;
        float horizontalInput = delta.x / Screen.width * horizontalDegreesPerScreen;
        float verticalInput = -delta.y / Screen.height * verticalDegreesPerScreen;

        InputAxis horizontalAxis = orbitalFollow.HorizontalAxis;
        horizontalAxis.Value = horizontalAxis.ClampValue(horizontalAxis.Value + horizontalInput);
        orbitalFollow.HorizontalAxis = horizontalAxis;

        InputAxis verticalAxis = orbitalFollow.VerticalAxis;
        verticalAxis.Value =verticalAxis.ClampValue(verticalAxis.Value + verticalInput);
        orbitalFollow.VerticalAxis = verticalAxis;
    }
}