using System;
using UnityEngine;

public class MapActivity : MonoBehaviour
{
    private MapManager mapManager;
    private int mapIndex = -1;

    private bool isActive;
    private double activatedServerTime = -1d;

    public bool IsActive => isActive;
    public double ActivatedServerTime => activatedServerTime;

    public event Action<bool> ActivityChanged;

    public MapManager Manager => mapManager;
    public int MapIndex => mapIndex;

    public void Initialize(MapManager manager, int index)
    {
        mapManager = manager;
        mapIndex = index;
    }

    public void RequestEnter()
    {
        if (mapManager == null || mapIndex < 0)
            return;

        mapManager.RequestMapActivityChange(mapIndex, true);
    }

    public void RequestExit()
    {
        if (mapManager == null || mapIndex < 0)
            return;

        mapManager.RequestMapActivityChange(mapIndex, false);
    }

    internal void ApplyNetworkState(bool active, double serverActivationTime)
    {
        bool wasActive = isActive;

        isActive = active;
        activatedServerTime = serverActivationTime;

        if (wasActive != isActive)
        {
            ActivityChanged?.Invoke(isActive);
        }
    }
}