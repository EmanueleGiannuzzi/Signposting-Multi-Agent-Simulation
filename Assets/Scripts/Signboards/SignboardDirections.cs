using System.Collections.Generic;
using System.Linq;
using Agents.Wanderer;
using UnityEngine;
using UnityEngine.AI;
using Vertx.Debugging;

[RequireComponent(typeof(IFCSignBoard))]
public class SignboardDirections : MonoBehaviour {
    [SerializeField] private List<WandererGoal> destinations = new();

    private bool getDirectionFromDestination(WandererGoal destination, out Vector3 direction) {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(this.transform.position, destination.Position, NavMesh.AllAreas, path);
        bool pathExists = path.status == NavMeshPathStatus.PathComplete;
        if (pathExists) {
            IRouteMarker marker = getFirstMarkerOnPath(path);
            //TODO: Debug line
            direction = marker?.Position ?? destination.Position;
            return true;
        }

        direction = Vector3.zero;
        return false;
    }

    public bool TryGetDirection(WandererGoal destination, out Vector3 direction) {
        if (destinations.Contains(destination)) {
            return getDirectionFromDestination(destination, out direction);
        }
        direction = Vector3.zero;
        return false;
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

    private void OnDrawGizmos() {
        if (!destinations.Any()) {
            return;
        }
        
        Vector3 signPosition = this.transform.position;
        foreach (WandererGoal destination in destinations) {
            if(destination == null) continue;

            if (getDirectionFromDestination(destination, out Vector3 directionPointed)) {
                Quaternion destinationDirection = Quaternion.LookRotation(directionPointed - signPosition);
                D.raw(new Shape.Arrow(signPosition, destinationDirection, 0.5f), Color.green);
            }
            else {
                Quaternion destinationDirection = Quaternion.LookRotation(destination.Position - signPosition);
                D.raw(new Shape.Arrow(signPosition, destinationDirection, 0.5f), Color.red);
            }
        }
    }
}
