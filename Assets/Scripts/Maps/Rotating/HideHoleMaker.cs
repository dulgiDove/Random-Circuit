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

    [Header("Safety")]
    [SerializeField]
    private int maxAttempts = 10000;

    private void Start()
    {
        ClearGeneratedHideHoles();
        GenerateHideHoles();
    }


    [ContextMenu("Generate Hide Holes")]
    public void GenerateHideHoles()
    {
        if (hideHolePrefab == null)
        {
            return;
        }

        List<Vector3> positions = new List<Vector3>();
        int attempts = 0;

        while (positions.Count < holeCount && attempts < maxAttempts)
        {
            attempts++;

            Vector2 randomPoint = Random.insideUnitCircle * radius;
            Vector3 position = transform.position + new Vector3(randomPoint.x, yOffset, randomPoint.y);

            if (!IsValidPosition(position, positions))
            {
                continue;
            }

            positions.Add(position);

            GameObject hole = Instantiate(
                    hideHolePrefab,
                    position,
                    hideHolePrefab.transform.rotation,
                    transform
                );

            hole.name = $"HideHole_{positions.Count:00}";
        }
    }


    private bool IsValidPosition(Vector3 candidate, List<Vector3> positions)
    {
        float minDistanceSqr = minDistance * minDistance;

        foreach (Vector3 position in positions)
        {
            Vector2 a = new Vector2(candidate.x, candidate.z);
            Vector2 b = new Vector2(position.x, position.z);

            if ((a - b).sqrMagnitude < minDistanceSqr)
            {
                return false;
            }
        }

        return true;
    }


    [ContextMenu("Clear Generated Hide Holes")]
    public void ClearGeneratedHideHoles()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        const int segments = 64;
        Vector3 previousPoint = transform.position + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i / (float)segments * Mathf.PI * 2f;

            Vector3 nextPoint = transform.position + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }
}