using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class MarkerGenerator : MonoBehaviour {
    public GameObject ifcGameObject;
    public Material markerMaterial;

    private GameObject markerParent;

    public string[] IfcTraversableTags = { "IfcDoor" };
    
    public delegate void OnMarkersGenerated(List<IRouteMarker> markers);
    public event OnMarkersGenerated OnMarkersGeneration;
    
    private readonly List<IRouteMarker> markers = new();
    public bool Ready {get; private set;} = false;
    public List<IRouteMarker> Markers => Ready ? markers : null;

    private void Start() {
        AddMarkersToTraversables();
    }

    private bool isTraversableTag(string ifcTag) {
        return IfcTraversableTags.Contains(ifcTag);
    }

    private IEnumerable<IFCData> ifcTraversables() {
        return ifcGameObject.GetComponentsInChildren<IFCData>().Where(ifcData => isTraversableTag(ifcData.IFCClass));
    }

    public void AddMarkersToTraversables() {
        resetMarkers();

        if (!ifcGameObject) {
            Debug.LogError("[MarkerGenerator]: No IFC Object found");
            return;
        }

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
            if(traversableCenterProjectionOnNavMesh(traversableCenter, out Vector3 projectionOnNavmesh)
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

    private static bool traversableCenterProjectionOnNavMesh(Vector3 traversableCenter, out Vector3 result) {
        if (NavMesh.SamplePosition(traversableCenter, out NavMeshHit hit, 2.5f, NavMesh.AllAreas)) {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }

    private void resetMarkers() {
        foreach (var markerGroup in GameObject.FindGameObjectsWithTag(Constants.MARKERS_TAG)) {
            DestroyImmediate(markerGroup);
        }
        markerParent = new GameObject(Constants.MARKERS_GROUP_TAG) {
            tag = Constants.MARKERS_TAG
        };
    }

    private IntermediateMarker spawnMarker(Vector3 pos, float widthX, float widthZ, string name) {
        // const float MIN_SIZE = 1.5f;
        // widthX = Mathf.Max(MIN_SIZE, widthX);
        // widthZ = Mathf.Max(MIN_SIZE, widthZ);
        
        GameObject markerGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        markerGO.transform.parent = markerParent.transform;
        pos += new Vector3(0f, 0.01f, 0f);
        markerGO.transform.position = pos;
        markerGO.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        markerGO.transform.localScale = new Vector3(widthX, widthZ, 1.6f);
        markerGO.GetComponent<Renderer>().sharedMaterial = markerMaterial;
        markerGO.layer = Constants.MARKERS_LAYER;
        markerGO.name = name;
        IntermediateMarker marker = markerGO.AddComponent<IntermediateMarker>();
        MeshCollider markerCollider = markerGO.GetComponent<MeshCollider>();
        markerCollider.convex = true;
        markerCollider.isTrigger = true;

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
    public IRouteMarker GetClosestMarkerToPoint(Vector3 point) {
        if (!Ready) {
            return null;
        }
        
        IRouteMarker closestMarker = null;
        float closestDistanceSqr = Mathf.Infinity;
        foreach (IRouteMarker marker in markers) {
            Vector3 directionToTarget = marker.Position - point;
            float distanceSqr = directionToTarget.sqrMagnitude;
            if(distanceSqr < closestDistanceSqr) {
                closestDistanceSqr = distanceSqr;
                closestMarker = marker;
            }
        }
        return closestMarker;
    }
}
