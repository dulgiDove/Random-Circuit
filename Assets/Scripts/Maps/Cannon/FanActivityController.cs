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
        mapActivity = GetComponentInParent<MapActivity>();
    }

    private void OnEnable()
    {
        if (mapActivity == null)
        {
            SetActiveState(true);
            return;
        }

        mapActivity.ActivityChanged += OnActivityChanged;
        SetActiveState(mapActivity.IsActive);
    }

    private void OnDisable()
    {
        if (mapActivity != null)
        {
            mapActivity.ActivityChanged -= OnActivityChanged;
        }
    }

    private void OnActivityChanged(bool active)
    {
        SetActiveState(active);
    }

    private void SetActiveState(bool active)
    {
        if (modelRotator != null)
        {
            modelRotator.enabled = active;
        }

        if (tornadoVfx != null)
        {
            tornadoVfx.SetActive(active);
        }
    }
}