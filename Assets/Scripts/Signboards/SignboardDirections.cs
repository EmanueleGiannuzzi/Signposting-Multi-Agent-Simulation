using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(IFCSignBoard))]
public class SignboardDirections : MonoBehaviour {
    [SerializeField] private List<Vector3> destinations = new();

    public static readonly Vector3 NO_DIRECTION = Vector3.negativeInfinity;

    private Vector3 getDirectionFromDestination(Vector3 destination) {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(this.transform.position, destination, NavMesh.AllAreas, path);
        bool pathExists = path.status == NavMeshPathStatus.PathComplete;
        if (pathExists) {
            IRouteMarker marker = getFirstMarkerOnPath(path);
            //TODO: Debug line
            return marker?.Position ?? destination;
        }
        return NO_DIRECTION;
    }

    public Vector3 GetDirection(Vector3 destination) {
        return destinations.Contains(destination) ? getDirectionFromDestination(destination) : NO_DIRECTION;
    }
    
    private static IRouteMarker getFirstMarkerOnPath(NavMeshPath path) {
        const float SPHERE_RADIUS = 0.5f;

        for (int i = 1; i < path.corners.Length; i++) {
            Vector3 directionTowardsNextCorner = (path.corners[i - 1] - path.corners[i]).normalized;
            float distanceToNextCorner = Vector3.Distance(path.corners[i - 1], path.corners[i]);
            
            if (Physics.SphereCast(path.corners[i], SPHERE_RADIUS, directionTowardsNextCorner, out RaycastHit hit, distanceToNextCorner + 0.3f, Constants.ONLY_MARKERS_LAYER_MASK)) {
                IRouteMarker markerHit = hit.collider.GetComponent<IRouteMarker>();
                if (markerHit != null) {
                    return markerHit;
                }
            }
        }
        return null;
    }
}
