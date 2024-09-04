using System.Collections.Generic;
using System.Linq;
using Agents.Wanderer;
using UnityEngine;
using UnityEngine.AI;
using Vertx.Debugging;

[RequireComponent(typeof(IFCSignBoard))]
public class SignboardDirections : MonoBehaviour {
    private readonly bool DEBUG = true;
    
    [SerializeField] private List<WandererGoal> destinations = new();

    private bool getDirectionFromDestination(WandererGoal destination, out IRouteMarker direction) {
        NavMeshPath path = new NavMeshPath();
        MarkerGenerator.TraversableCenterProjectionOnNavMesh(this.transform.position, out Vector3 startPos);
        MarkerGenerator.TraversableCenterProjectionOnNavMesh(destination.GetPosition(), out Vector3 destinationPos);
        NavMesh.CalculatePath(startPos, destinationPos, NavMesh.AllAreas, path);
        
        bool pathExists = path.status == NavMeshPathStatus.PathComplete;
        if (pathExists) {
            // if (DEBUG) {//TODO: Remove
            //     Vector3 prevCorner = this.transform.position;
            //     foreach (Vector3 corner in path.corners) {
            //         Debug.DrawLine(prevCorner, corner, Color.blue);
            //         prevCorner = corner;
            //     }
            // }
            
            IRouteMarker marker = getFirstMarkerOnPath(path);
            direction = marker ?? destination;
            return true;
        }

        direction = null;
        return false;
    }

    public bool TryGetDirection(WandererGoal destination, out IRouteMarker direction) {
        if (destinations.Contains(destination)) {
            return getDirectionFromDestination(destination, out direction);
        }
        direction = null;
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
        if (!DEBUG || !destinations.Any()) {
            return;
        }
        
        Vector3 signPosition = this.transform.position;
        foreach (WandererGoal destination in destinations) {
            if(destination == null) continue;

            if (getDirectionFromDestination(destination, out IRouteMarker directionPointed)) {
                Quaternion destinationDirection = Quaternion.LookRotation(directionPointed.Position - signPosition);
                D.raw(new Shape.Arrow(signPosition, destinationDirection, 0.5f), Color.green);
            }
            else {
                Quaternion destinationDirection = Quaternion.LookRotation(destination.GetPosition() - signPosition);
                D.raw(new Shape.Arrow(signPosition, destinationDirection, 0.5f), Color.red);
            }
        }
    }
}
