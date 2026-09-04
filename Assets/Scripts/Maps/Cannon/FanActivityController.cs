using UnityEngine;

public class FanActivityController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private FanModelRotator modelRotator;
    [SerializeField]
    private GameObject tornadoVfx;

    private MapActivity mapActivity;

    private void Awake()
    {
        Debug.Assert(modelRotator != null);
        Debug.Assert(tornadoVfx != null);

        mapActivity = GetComponentInParent<MapActivity>();
        Debug.Assert(mapActivity != null);
    }

    private void OnEnable()
    {
        mapActivity.ActivityChanged += OnActivityChanged;
        SetActiveState(mapActivity.IsActive);
    }

    private void OnDisable()
    {
        mapActivity.ActivityChanged -= OnActivityChanged;
    }

    private void OnActivityChanged(bool active)
    {
        SetActiveState(active);
    }

    private void SetActiveState(bool active)
    {
        modelRotator.enabled = active;
        tornadoVfx.SetActive(active);
    }
}