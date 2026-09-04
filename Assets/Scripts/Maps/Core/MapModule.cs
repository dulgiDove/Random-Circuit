using UnityEngine;

public class MapModule : MonoBehaviour
{
    [Header("Points")]
    [SerializeField]
    private Transform entrancePoint;
    [SerializeField]
    private Transform exitPoint;

    public Transform EntrancePoint => entrancePoint;
    public Transform ExitPoint => exitPoint;

    private void Awake()
    {
        Debug.Assert(entrancePoint != null);
        Debug.Assert(exitPoint != null);
    }
}