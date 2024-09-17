using System;
using System.Collections.Generic;
using System.Linq;
using Agents.Navigation.Markers;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class MarkerGenerator : MonoBehaviour {
    public GameObject ifcGameObject;
    public Material markerMaterial;

    private GameObject markerParent;

    public string[] IfcTraversableTags = { "IfcDoor" };
    public string[] IfcStairsTags = { "IfcStair", "IfcStairFlight" };
    public string[] flooringIfcTags = { "IfcSlab" };
    
    public float StairSlopeThreshold = 10.0f;
    
    public delegate void OnMarkersGenerated(List<IRouteMarker> markers);
    public event OnMarkersGenerated OnMarkersGeneration;
    
    private readonly List<IRouteMarker> markers = new();
    public bool Ready {get; private set;} = false;
    public List<IRouteMarker> Markers => Ready ? markers : null;

    private void Start() {
        GenerateMarkers();
    }

    public void GenerateMarkers() {
        resetMarkers();

        if (!ifcGameObject) {
            Debug.LogError("[MarkerGenerator]: No IFC Object found");
            return;
        }
        
        addMarkersToTraversables();
        addMarkersToStairs();
    }

    private bool isTraversableTag(string ifcTag) {
        return IfcTraversableTags.Contains(ifcTag);
    }
    
    private bool isStairsTag(string ifcTag) {
        return IfcStairsTags.Contains(ifcTag);
    }

    private IEnumerable<IFCData> ifcTraversables() {
        return ifcGameObject.GetComponentsInChildren<IFCData>().Where(ifcData => isTraversableTag(ifcData.IFCClass));
    }
    
    private IEnumerable<IFCData> ifcStairs() {
        return ifcGameObject.GetComponentsInChildren<IFCData>().Where(ifcData => isStairsTag(ifcData.IFCClass));
    }
    
    private IEnumerable<IFCData> ifcFlooring() {
        return ifcGameObject.GetComponentsInChildren<IFCData>().Where(ifcData => flooringIfcTags.Contains(ifcData.IFCClass));
    }

    private void addMarkersToTraversables() {
        float progress = 0f;
        IEnumerable<IFCData> traversables = ifcTraversables();
        float progressBarStep = 1f / traversables.Count();

        int spawnedMarkers = 0;
        foreach (IFCData traversable in traversables) {
            progress -= progressBarStep;
            
            Renderer traversableRenderer = traversable.GetComponent<Renderer>();
            if (!traversableRenderer) {
                return;
            }
            
            Bounds traversableRendererBounds = traversableRenderer.bounds;
            Vector3 traversableCenter = traversableRendererBounds.center;

            const float MIN_SIZE = 0.5f;
            if(TraversableCenterProjectionOnNavMesh(traversableCenter, out Vector3 projectionOnNavmesh)
               && traversableCenter.y > projectionOnNavmesh.y) {
                float widthX = traversableRendererBounds.extents.x*2;
                float widthZ = traversableRendererBounds.extents.z*2;
                widthX = Mathf.Max(MIN_SIZE, widthX);
                widthZ = Mathf.Max(MIN_SIZE, widthZ);
                IntermediateMarker marker = spawnMarker(projectionOnNavmesh, widthX, widthZ, $"IntermediateMarker-{spawnedMarkers}");
                spawnedMarkers++;
                
                markers.Add(marker);
            } 
            
            if(EditorUtility.DisplayCancelableProgressBar("Marker Generator", $"Generating Routing Markers ({spawnedMarkers}/{traversables.Count()})", 1f - progress)) {
                EditorUtility.ClearProgressBar();
                return;
            }
        }
        EditorUtility.ClearProgressBar();
        
        Ready = true;
        OnMarkersGeneration?.Invoke(markers);
    }
    
    private void addMarkersToStairs() {
        IEnumerable<IFCData> stairObjects = ifcStairs();
        IEnumerable<IFCData> flooringObjectsList = ifcFlooring();
        
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        List<Tuple<Vector3, Vector3>> stairTransitionEdges = new (); // Store only the first point of each stair
        foreach (IFCData stair in stairObjects) {
            Bounds stairBounds = stair.GetComponent<Collider>().bounds;
            stairTransitionEdges.AddRange(DetectStairPointsInBounds(triangulation, stairBounds));
        }
        
        const float DISTANCE_FROM_FLOOR_THRESHOLD = 0.1f;
        stairTransitionEdges.RemoveAll(edge => {
            foreach (IFCData floor in flooringObjectsList) {
                float floorY = floor.GetComponent<Collider>().bounds.max.y;
                float edgeY = ((edge.Item1 + edge.Item2) / 2).y;
                if (Math.Abs(floorY - edgeY) <= DISTANCE_FROM_FLOOR_THRESHOLD) {
                    return false;
                }
            }
            return true;
        });
        Debug.Log($"Detected {stairTransitionEdges.Count} stair transition point{(stairTransitionEdges.Count > 1 ? "s" : "")}.");

        int spawnedMarkers = 0;

        List<LinkedMarker> stairMarkers = new ();
        foreach (Tuple<Vector3, Vector3> edge in stairTransitionEdges) {
            LinkedMarker stairMarker = spawnStairMarker(edge, $"StairMarker-{spawnedMarkers}");
            Markers.Add(stairMarker);
            stairMarkers.Add(stairMarker);
            spawnedMarkers++;
        }

        foreach (LinkedMarker stairMarker1 in stairMarkers) {
            foreach (LinkedMarker stairMarker2 in stairMarkers) {
                if(stairMarker1 == stairMarker2) continue;
                
                if (areMarkersOnDifferentFloors(stairMarker1, stairMarker2) &&
                    DoesDirectPathExistsBetweenPoints(stairMarker1, stairMarker2, out _)) {
                    stairMarker1.SetLinkedMarker(stairMarker2);
                }
            }
        }
    }

    private bool areMarkersOnDifferentFloors(IRouteMarker startMarker, IRouteMarker destinationMarker) {
        const float DISTANCE_THRESHOLD = 0.2f;
        
        IEnumerable<IFCData> flooringObjectsList = ifcFlooring();

        IFCData startMarkerFloor = null;
        IFCData destinationMarkerFloor = null;
        foreach (IFCData floor in flooringObjectsList) {
            float floorY = floor.GetComponent<Collider>().bounds.max.y;
            float startMarkerY = startMarker.Position.y;
            float destinationMarkerY = destinationMarker.Position.y;
            
            if (Math.Abs(startMarkerY - floorY) <= DISTANCE_THRESHOLD) {
                startMarkerFloor = floor;
            }
            if (Math.Abs(destinationMarkerY - floorY) <= DISTANCE_THRESHOLD) {
                destinationMarkerFloor = floor;
            }
        }
        
        return startMarkerFloor != null 
               && destinationMarkerFloor != null 
               && startMarkerFloor != destinationMarkerFloor;
    }

    private LinkedMarker spawnStairMarker(Tuple<Vector3, Vector3> edge, string name) {
        Vector3 center = (edge.Item1 + edge.Item2) / 2;
        Vector3 direction = (edge.Item2 - edge.Item1).normalized;
        Quaternion rotation = Quaternion.LookRotation(Vector3.down, direction);
        float distance = Vector3.Distance(edge.Item1, edge.Item2);
            
        GameObject markerGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        markerGO.transform.parent = markerParent.transform;
        center += new Vector3(0f, 0.01f, 0f);
        markerGO.transform.position = center;
        
        markerGO.transform.rotation = rotation;
        markerGO.transform.localScale = new Vector3(0.3f, distance, 1f);
        markerGO.GetComponent<Renderer>().sharedMaterial = markerMaterial;
        markerGO.layer = Constants.MARKERS_LAYER;
        markerGO.name = name;
        Utility.DestroyObject(markerGO.GetComponent<MeshCollider>());
        
        BoxCollider markerCollider = markerGO.AddComponent<BoxCollider>();
        markerCollider.isTrigger = true;
        markerCollider.size = new Vector3(1f, 1f, 0.1f);

        LinkedMarker marker = markerGO.AddComponent<LinkedMarker>();
        return marker;
    }

    private List<Tuple<Vector3, Vector3>> DetectStairPointsInBounds(NavMeshTriangulation triangulation, Bounds bounds) {
        List<Tuple<Vector3, Vector3>> stairTransitionPoints = new ();
        HashSet<Tuple<Vector3, Vector3>> processedEdges = new ();

        for (int i = 0; i < triangulation.indices.Length; i += 3) {
            Vector3 v1 = triangulation.vertices[triangulation.indices[i]];
            Vector3 v2 = triangulation.vertices[triangulation.indices[i + 1]];
            Vector3 v3 = triangulation.vertices[triangulation.indices[i + 2]];

            if (!(bounds.Contains(v1) || bounds.Contains(v2) || bounds.Contains(v3))) {
                continue;
            }

            Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;
            float slopeAngle = Vector3.Angle(normal, Vector3.up);

            if (slopeAngle > StairSlopeThreshold) {
                for (int j = 0; j < triangulation.indices.Length; j += 3) {
                    if (j == i) continue;

                    Vector3 adjV1 = triangulation.vertices[triangulation.indices[j]];
                    Vector3 adjV2 = triangulation.vertices[triangulation.indices[j + 1]];
                    Vector3 adjV3 = triangulation.vertices[triangulation.indices[j + 2]];

                    List<Vector3> sharedEdge = getSharedEdge(v1, v2, v3, adjV1, adjV2, adjV3);
                    if (sharedEdge.Count == 2) {
                        Vector3 adjNormal = Vector3.Cross(adjV2 - adjV1, adjV3 - adjV1).normalized;
                        float adjSlopeAngle = Vector3.Angle(adjNormal, Vector3.up);
                        bool isStartOfSlope = adjSlopeAngle < StairSlopeThreshold && slopeAngle > StairSlopeThreshold;
                        bool isEndOfSlope = adjSlopeAngle > StairSlopeThreshold && slopeAngle < StairSlopeThreshold;

                        if (isStartOfSlope || isEndOfSlope) {
                            Tuple<Vector3, Vector3> stairEdge = new Tuple<Vector3, Vector3>(sharedEdge[0], sharedEdge[1]);
                            if (!processedEdges.Contains(stairEdge)) {
                                stairTransitionPoints.Add(stairEdge);
                                processedEdges.Add(stairEdge);
                            }
                            break;
                        }
                    }
                }
            }
        }
        return stairTransitionPoints;
    }

    public static bool TraversableCenterProjectionOnNavMesh(Vector3 traversableCenter, out Vector3 result) { //TODO: Move to Utility
        if (NavMesh.SamplePosition(traversableCenter, out NavMeshHit hit, 2.5f, NavMesh.AllAreas)) {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }

    private void resetMarkers() {
        foreach (GameObject markerGroup in GameObject.FindGameObjectsWithTag(Constants.MARKERS_TAG)) {
            DestroyImmediate(markerGroup);
        }
        markerParent = new GameObject(Constants.MARKERS_GROUP_TAG) {
            tag = Constants.MARKERS_TAG
        };
    }

    private IntermediateMarker spawnMarker(Vector3 pos, float widthX, float widthZ, string objName) {
        GameObject markerGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        markerGO.transform.parent = markerParent.transform;
        pos += new Vector3(0f, 0.01f, 0f);
        markerGO.transform.position = pos;
        markerGO.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        markerGO.transform.localScale = new Vector3(widthX, widthZ, 1.6f);
        markerGO.GetComponent<Renderer>().sharedMaterial = markerMaterial;
        markerGO.layer = Constants.MARKERS_LAYER;
        markerGO.name = objName;
        Utility.DestroyObject(markerGO.GetComponent<MeshCollider>());
        
        BoxCollider markerCollider = markerGO.AddComponent<BoxCollider>();
        markerCollider.isTrigger = true;
        markerCollider.size = new Vector3(1f, 1f, 0.1f);

        IntermediateMarker marker = markerGO.AddComponent<IntermediateMarker>();
        return marker;
    }

    public static List<IRouteMarker> RemoveUnreachableMarkersFromPosition(IEnumerable<IRouteMarker> markers, Vector3 startPosition, bool drawDebug = false) {
        List<IRouteMarker> markersConnected = new (markers);
        markersConnected.RemoveAll(marker => {
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(startPosition, marker.Position, NavMesh.AllAreas, path);
            bool pathExists = path.status == NavMeshPathStatus.PathComplete;
            if (drawDebug && !pathExists) {
                Debug.DrawLine(marker.Position, startPosition, Color.red, 120f, false);
            }
            return !pathExists;
        });
        return markersConnected;
    }

    [CanBeNull]
    public IRouteMarker GetClosestMarkerToPoint(Vector3 point, float minDistance = 1f) {
        if (!Ready) {
            return null;
        }

        float minDistanceSqr = minDistance * minDistance;
        IRouteMarker closestMarker = null;
        float closestDistanceSqr = Mathf.Infinity;
        foreach (IRouteMarker marker in markers) {
            Vector3 directionToTarget = marker.Position - point;
            float distanceSqr = directionToTarget.sqrMagnitude;
            if(distanceSqr >= minDistanceSqr && distanceSqr < closestDistanceSqr) {
                closestDistanceSqr = distanceSqr;
                closestMarker = marker;
            }
        }
        return closestMarker;
    }
    
    private static List<Vector3> getSharedEdge(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 adjV1, Vector3 adjV2, Vector3 adjV3) {
        List<Vector3> sharedEdge = new List<Vector3>();

        if (isVertexShared(v1, adjV1, adjV2, adjV3)) sharedEdge.Add(v1);
        if (isVertexShared(v2, adjV1, adjV2, adjV3)) sharedEdge.Add(v2);
        if (isVertexShared(v3, adjV1, adjV2, adjV3)) sharedEdge.Add(v3);

        return sharedEdge;
    }

    private static bool isVertexShared(Vector3 v, Vector3 adjV1, Vector3 adjV2, Vector3 adjV3) {
        return v == adjV1 || v == adjV2 || v == adjV3;
    }
    
    
    public static bool DoesDirectPathExistsBetweenPoints(IRouteMarker startMarker, IRouteMarker destinationMarker, out float cost) {
        Vector3 start = startMarker.Position;
        Vector3 destination = destinationMarker.Position;
        
        NavMeshPath path = new ();
        NavMesh.CalculatePath(start, destination, NavMesh.AllAreas, path);
        cost = getPathLengthSquared(path);
        bool pathExists = path.status == NavMeshPathStatus.PathComplete;
        bool isDirect = !doesPathIntersectOtherMarkers(path, startMarker, destinationMarker);

        return pathExists && isDirect;
    }
    
    public static bool doesPathIntersectOtherMarkers(NavMeshPath path, IRouteMarker startMarker, IRouteMarker destinationMarker) {
        const float SPHERE_RADIUS = 0.5f;

        for (int i = 1; i < path.corners.Length; i++) {
            Vector3 directionTowardsNextCorner = (path.corners[i - 1] - path.corners[i]).normalized;
            float distanceToNextCorner = Vector3.Distance(path.corners[i - 1], path.corners[i]);
            //DrawPhysicsSettings.SetDuration(60f);
            if (Physics.SphereCast(path.corners[i], SPHERE_RADIUS, directionTowardsNextCorner, out RaycastHit hit, distanceToNextCorner + 0.3f, Constants.ONLY_MARKERS_LAYER_MASK)) {
                IRouteMarker markerHit = hit.collider.GetComponent<IRouteMarker>();
                if (markerHit != null && markerHit != startMarker && markerHit != destinationMarker) {
                    return true;
                }
            }
        }
        return false;
    }
    
    private static float getPathLengthSquared(NavMeshPath path) {
        Vector3[] corners = path.corners;

        float length = 0f;
        for (int i = 1; i < corners.Length; i++) {
            length += (corners[i] -  corners[i - 1]).sqrMagnitude;
        }

        return length;
    }
    
}
