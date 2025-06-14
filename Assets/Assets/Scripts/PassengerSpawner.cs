using System.Collections.Generic;
using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    [Header("Prefabs & Tags")]
    public GameObject passengerPrefab;
    public string crossIntersectionTag = "CrossIntersection";

    [Header("Raycast Ground Snapping")]
    public float raycastHeight = 5f;
    public LayerMask groundLayer;

    [Header("Road Settings")]
    public string roadParentName = "Roads";
    public float connectionCheckDistance = 0.5f;

    [Header("Passenger Settings")]
    public Vector3 passengerScale = new Vector3(0.0766f, 0.0766f, 0.0766f);

    private List<Vector3> passengerPositions = new List<Vector3>();
    private Transform passengerParent;

    void Start()
    {
        SpawnPassengersOnOpenConnections();
    }

    void SpawnPassengersOnOpenConnections()
    {
        passengerPositions.Clear();

        // Clean up old passengers group
        Transform existingGroup = transform.Find("Passengers");
        if (existingGroup != null) DestroyImmediate(existingGroup.gameObject);

        // Create new parent group
        passengerParent = new GameObject("Passengers").transform;
        passengerParent.parent = transform;

        GameObject[] intersections = GameObject.FindGameObjectsWithTag(crossIntersectionTag);
        if (intersections.Length == 0)
        {
            Debug.LogWarning("No intersections found with tag: " + crossIntersectionTag);
            return;
        }

        // Get all road connection points
        HashSet<Vector3> roadConnectionPoints = new HashSet<Vector3>();
        GameObject roadParent = GameObject.Find(roadParentName);
        if (roadParent != null)
        {
            foreach (Transform road in roadParent.transform)
            {
                foreach (Transform cp in road)
                {
                    if (cp.name.ToLower().StartsWith("connectionpoint"))
                        roadConnectionPoints.Add(cp.position);
                }
            }
        }

        int passengerCount = 0;

        foreach (GameObject intersection in intersections)
        {
            Vector3 intersectionCenter = intersection.transform.position;

            Transform[] children = intersection.GetComponentsInChildren<Transform>();
            foreach (Transform cp in children)
            {
                if (!cp.name.ToLower().StartsWith("connectionpoint"))
                    continue;

                // Check if this connection is already connected to a road
                bool isConnected = false;
                foreach (Vector3 roadCP in roadConnectionPoints)
                {
                    if (Vector3.Distance(cp.position, roadCP) < connectionCheckDistance)
                    {
                        isConnected = true;
                        break;
                    }
                }

                if (isConnected)
                    continue;

                // Cast ray downward to find ground
                Vector3 rayOrigin = cp.position + Vector3.up * raycastHeight;
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundLayer))
                {
                    Vector3 spawnPos = hit.point;

                    // Face toward the intersection (road)
                    Vector3 lookDirection = (intersectionCenter - spawnPos).normalized;
                    Quaternion rotation = Quaternion.LookRotation(lookDirection, Vector3.up);

                    GameObject passenger = Instantiate(passengerPrefab, spawnPos, rotation, passengerParent);
                    passenger.transform.localScale = passengerScale;
                    passengerPositions.Add(passenger.transform.position);
                    passengerCount++;
                }
                else
                {
                    Debug.LogWarning($"No ground hit for {cp.name} at {cp.position}");
                }
            }
        }

        Debug.Log($"✅ Spawned {passengerCount} passengers.");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (Vector3 pos in passengerPositions)
        {
            Gizmos.DrawSphere(pos, 0.2f);
        }
    }
}
