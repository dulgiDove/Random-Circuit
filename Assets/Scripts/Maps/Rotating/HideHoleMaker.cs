using System;
using System.Collections.Generic;
using UnityEngine;

public class HideHoleMaker : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField]
    private GameObject hideHolePrefab;

    [Header("Generation")]
    [SerializeField]
    private int holeCount = 20;
    [SerializeField]
    private float radius = 45f;
    [SerializeField]
    private float minDistance = 5f;
    [SerializeField]
    private float yOffset = 0.02f;

    [Header("Random")]
    [SerializeField]
    private int seed = 12345;

    [Header("Safety")]
    [SerializeField]
    private int maxAttempts = 10000;

    private void Awake()
    {
        Debug.Assert(hideHolePrefab != null);
    }

    private void Start()
    {
        GenerateHideHoles();
    }

    private void GenerateHideHoles()
    {
        List<Vector3> positions = new List<Vector3>(holeCount);
        System.Random random = new System.Random(seed);

        int attempts = 0;

        while (positions.Count < holeCount && attempts < maxAttempts)
        {
            attempts++;

            Vector2 randomPoint = GetRandomPointInCircle(random) * radius;
            Vector3 localPosition = new Vector3(randomPoint.x, yOffset, randomPoint.y);
            Vector3 worldPosition = transform.TransformPoint(localPosition);

            if (!IsValidPosition(worldPosition, positions))
                continue;

            int networkId = positions.Count;

            positions.Add(worldPosition);

            GameObject hole = Instantiate(
                hideHolePrefab,
                worldPosition,
                hideHolePrefab.transform.rotation,
                transform
            );

            hole.name = $"HideHole_{networkId:00}";

            HideHole hideHole = hole.GetComponent<HideHole>();

            if (hideHole == null)
                return;

            hideHole.InitializeNetworkId(networkId);
        }
    }

    private Vector2 GetRandomPointInCircle(System.Random random)
    {
        float angle = (float)random.NextDouble() * Mathf.PI * 2f;
        float distance = Mathf.Sqrt((float)random.NextDouble());

        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
    }

    private bool IsValidPosition(Vector3 candidate, List<Vector3> positions)
    {
        float minDistanceSqr = minDistance * minDistance;

        foreach (Vector3 position in positions)
        {
            Vector2 candidateXZ = new Vector2(candidate.x, candidate.z);
            Vector2 positionXZ = new Vector2(position.x, position.z);

            if ((candidateXZ - positionXZ).sqrMagnitude < minDistanceSqr)
            {
                return false;
            }
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        const int segments = 64;

        Vector3 previousPoint = transform.TransformPoint(new Vector3(radius, 0f, 0f));

        for (int i = 1; i <= segments; i++)
        {
            float angle = i / (float)segments * Mathf.PI * 2f;

            Vector3 nextPoint = transform.TransformPoint(new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));

            Gizmos.DrawLine(previousPoint, nextPoint);

            previousPoint = nextPoint;
        }
    }
}