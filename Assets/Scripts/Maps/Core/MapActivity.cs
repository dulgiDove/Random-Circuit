using System;
using UnityEngine;

public class MapActivity : MonoBehaviour
{
    private int activePlayerCount;
    public int ActivePlayerCount => activePlayerCount;

    public bool IsActive => activePlayerCount > 0;

    public event Action<bool> ActivityChanged;

    public void EnterPlayer()
    {
        bool wasActive = IsActive;
        activePlayerCount++;

        if (wasActive != IsActive)
        {
            ActivityChanged?.Invoke(IsActive);
        }
    }

    public void ExitPlayer()
    {
        bool wasActive = IsActive;

        activePlayerCount = Mathf.Max(activePlayerCount - 1, 0);

        if (wasActive != IsActive)
        {
            ActivityChanged?.Invoke(IsActive);
        }
    }
}