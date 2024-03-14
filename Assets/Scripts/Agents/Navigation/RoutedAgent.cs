using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent), typeof(Agent))]
public class RoutedAgent : MonoBehaviour {
    private const bool DEBUG = true;
    
    private NavMeshAgent navMeshAgent;
    private static AgentsHandler agentsHandler;

    private Queue<IRouteMarker> route;
    public float Error { get; set; } = 0f;

    private void Awake() {
        agentsHandler ??= FindObjectOfType<AgentsHandler>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void OnTriggerEnter(Collider other) {
        IRouteMarker marker = other.GetComponent<IRouteMarker>();
        if (marker == null || route is not { Count: > 0 } || marker != route.Peek()) {
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
        SetDestination(route.Peek().Position);
    }

    private void OnExitReached([NotNull] IRouteMarker exit) {
        agentsHandler.DestroyAgent(this.GetComponent<Agent>());
    }

    public void SetRoute(Queue<IRouteMarker> newRoute) {
        route = newRoute;
        if (newRoute.TryPeek(out IRouteMarker destination)) {
            SetDestination(destination.Position);
        }
    }

    public void SetDestination(Vector3 destination) {
        if (Error != 0f) {
            float randXOffset = Random.Range(-Error, Error);
            float randZOffset = Random.Range(-Error, Error);
            destination = new Vector3(destination.x + randXOffset, destination.y, destination.z + randZOffset);
        }
        navMeshAgent.SetDestination(destination);
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
            Gizmos.DrawLine(transform.position, navMeshAgent.destination);
        }
    }
}
