using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Maps")]
    [SerializeField]
    private List<MapModule> mapPrefabs;
    [SerializeField]
    private int mapCount = 3;

    [Header("Player")]
    [SerializeField]
    private PlayerMovement player;

    [Header("Goal")]
    [SerializeField]
    private GameObject goalLinePrefab;
    [SerializeField]
    private float goalDistance = 3f;

    private readonly List<MapModule> spawnedMaps = new List<MapModule>();

    private void Start()
    {
        GenerateMaps();
    }

    private void GenerateMaps()
    {
        if (mapPrefabs.Count < mapCount)
        {
            return;
        }

        List<MapModule> candidates = new List<MapModule>(mapPrefabs);

        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            MapModule temp = candidates[i];
            candidates[i] = candidates[randomIndex];
            candidates[randomIndex] = temp;
        }

        Vector3 nextEntrancePosition = Vector3.zero;

        for (int i = 0; i < mapCount; i++)
        {
            MapModule map = Instantiate(
                    candidates[i],
                    nextEntrancePosition,
                    Quaternion.identity,
                    transform
                );

            spawnedMaps.Add(map);

            nextEntrancePosition = map.ExitPoint.position;
        }

        MovePlayerToStart();

        CreateGoalLine();
    }


    private void MovePlayerToStart()
    {
        if (player == null || spawnedMaps.Count == 0)
        {
            return;
        }

        Transform entrance = spawnedMaps[0].EntrancePoint;
        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.position = entrance.position;

        if (controller != null)
        {
            controller.enabled = true;
        }
    }


    private void CreateGoalLine()
    {
        if (goalLinePrefab == null || spawnedMaps.Count == 0)
        {
            return;
        }

        Transform finalExit = spawnedMaps[spawnedMaps.Count - 1].ExitPoint;
        Vector3 goalPosition = finalExit.position + finalExit.forward * goalDistance + Vector3.up * 0.01f;

        Instantiate(
            goalLinePrefab,
            goalPosition,
            finalExit.rotation,
            transform
        );
    }
}