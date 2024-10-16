using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Agent))]
public class AgentRouted : MarkersAwareAgent {
    private readonly bool DEBUG = true;
    public float DestinationPositionError { get; set; } = 0f;
    
    private Queue<IRouteMarker> route;

    protected override void Awake() {
        MarkerReachedEvent += OnMarkerReachedEvent;
    }

    private void OnMarkerReachedEvent(IRouteMarker marker) {
        if (route is not { Count: > 0 } || marker != route.Peek()) {
            return;
        }
        IRouteMarker reachedMarker = route.Dequeue();
        if (route.Count > 0) {
            OnIntermediateMarkerReached(reachedMarker);
        }
        else {
            OnExitReached(reachedMarker);
        }
    }

    private void OnIntermediateMarkerReached([NotNull] IRouteMarker marker) {
        SetDestinationWithError(route.Peek().Position);
    }

    private void OnExitReached([NotNull] IRouteMarker exit) {
        agent.DestroyAgent();
    }
    
    
    public void SetDestinationWithError(Vector3 destination) {
        if (DestinationPositionError != 0f) {
            float randXOffset = Random.Range(-DestinationPositionError, DestinationPositionError);
            float randZOffset = Random.Range(-DestinationPositionError, DestinationPositionError);
            destination = new Vector3(destination.x + randXOffset, destination.y, destination.z + randZOffset);
        }
        agent.SetDestination(destination);
    }

    public void SetRoute(Queue<IRouteMarker> newRoute) {
        route = newRoute;
        if (newRoute.TryPeek(out IRouteMarker destination)) {
            SetDestinationWithError(destination.Position);
        }
    }
    public void AddDestination(IRouteMarker newDestination) {
        route.Enqueue(newDestination);
    }

    private void OnDrawGizmos() {
        if (!DEBUG) {
            return;
        }
        
        if (route?.Count > 0) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, agent.currentDestination);
        }
    }
}
