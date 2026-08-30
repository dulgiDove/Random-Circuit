using UnityEngine;

public class MapModule : MonoBehaviour
{
    [SerializeField]
    private Transform entrancePoint;
    [SerializeField]
    private Transform exitPoint;

    public Transform EntrancePoint => entrancePoint;
    public Transform ExitPoint => exitPoint;
}