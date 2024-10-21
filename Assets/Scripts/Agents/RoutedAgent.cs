using System.Collections.Generic;
using Agents;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MarkersAwareAgent))]
public class RoutedAgent : MonoBehaviour, IAgentWithGoal{
    private Agent agent;
    private MarkersAwareAgent markersAwareAgent;

    private Queue<IRouteMarker> route;
    [SerializeField] private float destinationPositionError = 0f;

    private void Awake() {
        agent = GetComponent<Agent>();
        markersAwareAgent = GetComponent<MarkersAwareAgent>();
        markersAwareAgent.MarkerReachedEvent += onMarkerReached;
    }

    private void onMarkerReached(IRouteMarker marker) {
        if (marker == CurrentGoal() ||
            (CurrentGoal() != null && marker.GetGameObject().Equals(CurrentGoal().GetGameObject()))) {
            RemoveCurrentGoal();
            if (GoalCount() > 0) {
                OnIntermediateMarkerReached();
            }
            else {
                OnExitReached();
            }
        }
    }

    private void OnIntermediateMarkerReached() {
        SetDestination(route.Peek().Position);
    }

    private void OnExitReached() {
        agent.DestroyAgent();
    }

    public void SetRoute(Queue<IRouteMarker> newRoute) {
        route = newRoute;
    }

    public void SetDestination(Vector3 destination) {
        if (destinationPositionError != 0f) {
            float randXOffset = Random.Range(-destinationPositionError, destinationPositionError);
            float randZOffset = Random.Range(-destinationPositionError, destinationPositionError);
            destination = new Vector3(destination.x + randXOffset, destination.y, destination.z + randZOffset);
        }
        
        if (MarkerGenerator.TraversableCenterProjectionOnNavMesh(destination, out Vector3 destinationPositionOnNavmesh)) {
            destination = destinationPositionOnNavmesh;
        }
        agent.SetDestination(destination);
    }

    public void AddDestination(IRouteMarker newDestination) {
        route.Enqueue(newDestination);
    }

    public void AddGoal(IRouteMarker goal) {
        route.Enqueue(goal);
    }

    public IRouteMarker CurrentGoal() {
        route.TryPeek(out IRouteMarker destination);
        return destination;
    }

    public int GoalCount() {
        return route.Count;
    }

    public void ClearGoals() {
        route.Clear();
    }

    public IRouteMarker RemoveCurrentGoal() {
        return route.Dequeue();
    }

    public virtual void StartTasks() {
        if (route.TryPeek(out IRouteMarker destination)) {
            SetDestination(destination.Position);
        }
    }
}
