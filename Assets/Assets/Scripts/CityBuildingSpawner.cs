using UnityEngine;
using System.Collections.Generic;

public class CityBuildingSpawner : MonoBehaviour
{
    [Header("Building Settings")]
    public GameObject buildingPrefab;
    public float offsetFromRoad = 10f;
    public float spacing = 10f;
    public float randomSpread = 5f;
    public float minDistanceBetweenBuildings = 15f;
    [Range(0f, 1f)] public float spawnChance = 0.8f;

    [Header("Terrain Settings")]
    public GameObject terrainPrefab;

    private Transform buildingParent;
    private Transform terrainGroup;
    private List<Vector3> placedPositions = new List<Vector3>();

    [ContextMenu("Spawn Buildings and Terrain")]
    public void SpawnBuildings()
    {
        if (buildingPrefab == null)
        {
            Debug.LogWarning("No building prefab assigned.");
            return;
        }

        // Clean up old buildings
        Transform existingBuildings = transform.Find("Buildings");
        if (existingBuildings != null) DestroyImmediate(existingBuildings.gameObject);

        buildingParent = new GameObject("Buildings").transform;
        buildingParent.parent = transform;
        placedPositions.Clear();

        // Handle terrain
        terrainGroup = transform.Find("Terrain");
        if (terrainGroup == null)
        {
            terrainGroup = new GameObject("Terrain").transform;
            terrainGroup.parent = transform;

            if (terrainPrefab != null)
            {
                GameObject terrain = Instantiate(terrainPrefab, transform.position, Quaternion.identity, terrainGroup);
                terrain.name = "Terrain";
            }
        }

        int buildingsPlaced = 0;

        foreach (Transform road in transform)
        {
            if (road.name == "Trees" || road.name == "Terrain" || road.name == "Buildings") continue;

            Vector3 forward = road.forward.normalized;
            Vector3 side = Vector3.Cross(Vector3.up, forward).normalized;

            for (float d = -10f; d <= 10f; d += spacing)
            {
                Vector3 basePos = road.position + forward * d;

                for (int i = -1; i <= 1; i += 2) // both sides
                {
                    if (Random.value > spawnChance) continue;

                    Vector3 spawnOffset = side * i * offsetFromRoad;
                    Vector3 randomJitter = new Vector3(
                        Random.Range(-randomSpread, randomSpread),
                        0,
                        Random.Range(-randomSpread, randomSpread)
                    );

                    Vector3 spawnPos = basePos + spawnOffset + randomJitter;

                    // Check for overlap
                    bool tooClose = false;
                    foreach (var pos in placedPositions)
                    {
                        if (Vector3.Distance(pos, spawnPos) < minDistanceBetweenBuildings)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (tooClose) continue;

                    GameObject building = Instantiate(buildingPrefab, spawnPos, Quaternion.identity, buildingParent);

                    float yaw = Quaternion.LookRotation(-side * i, Vector3.up).eulerAngles.y;
                    building.transform.rotation = Quaternion.Euler(-90f, yaw, 0f);
                    building.transform.localScale = new Vector3(25f, 25f, 25f);

                    placedPositions.Add(spawnPos);
                    buildingsPlaced++;
                }
            }
        }

        Debug.Log($"? Buildings placed: {buildingsPlaced}");
    }
}
