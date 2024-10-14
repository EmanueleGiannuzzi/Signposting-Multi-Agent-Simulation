using System.Collections.Generic;
using Agents;
using Agents.Navigation.Markers;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AgentWanderer : MarkersAwareAgent, IAgentWithGoal {
    private readonly bool DEBUG = true;

    private int _agentTypeID = -1;
    public int agentTypeID {
        get {
            if (_agentTypeID < 0) {
                selectAgentID();
            }
            return _agentTypeID;
        }
        private set => _agentTypeID = value;
    }

    private static VisibilityHandler visibilityHandler;
    private float agentFOV => agent.AgentFOVDegrees;

    private bool checkDestinationDistance = false;
    private float stopDistanceSqr = 0f;

    private float startingTime;
    public float TotalRunningTime => Time.time - startingTime;
    private float preferredDirectionTimeReceived;
    [SerializeField] private float preferredDirectionDecayTime = 60f;
    private float preferredDirectionAge;
    private Vector2 _preferredDirection;
    public Vector2 PreferredDirection {
        get => _preferredDirection;
        set {
            preferredDirectionTimeReceived = Time.time;
            _preferredDirection = value;
        }
    }

    public bool HasPreferredDirection => PreferredDirection != Vector2.zero;

    private readonly Queue<IRouteMarker> goals = new ();
    private IRouteMarker destinationMarker;
    public Vector3 CurrentDestination => agent.navMeshAgent.destination;
    public readonly HashSet<IFCSignBoard> VisitedSigns = new ();
    
    public delegate void OnWithinDestinationRange();
    private OnWithinDestinationRange _withinDestinationRangeDelegate;

    public delegate void OnDestinationMarkerReached([NotNull] IRouteMarker marker);
    public event OnDestinationMarkerReached DestinationMarkerReachedEvent;
    
    public delegate void OnGoalReached(bool isLastGoal, bool success);
    public event OnGoalReached GoalReachedEvent;

    protected override void Awake() {
        base.Awake();
        visibilityHandler ??= FindObjectOfType<VisibilityHandler>();
        if (!visibilityHandler) {
            Debug.LogError($"Unable to find {nameof(VisibilityHandler)}");
        }
    }

    private void selectAgentID() {
        agentTypeID = Random.Range(0, visibilityHandler.agentTypes.Length);
    }

    private void Start() {
        // agent.SetModelHeight(agentHeight); //TODO: Fix
    }

    private void FixedUpdate() {
        if (checkDestinationDistance) {
            if ((this.transform.position - agent.currentDestination).sqrMagnitude < stopDistanceSqr) {
                onWithinDestinationRange();
            }
        }

        if (HasPreferredDirection && Time.time - preferredDirectionTimeReceived > preferredDirectionDecayTime) {
            ResetPreferredDirection();
        } 
    }

    public void AddGoal(IRouteMarker goal) {
        goals.Enqueue(goal);
    }

    public IRouteMarker CurrentGoal() {
        return goals.Peek();
    }

    public int GoalCount() {
        return goals.Count;
    }

    public void ClearGoals() {
        goals.Clear();
    }

    public IRouteMarker RemoveCurrentGoal() {
        return goals.Dequeue();
    }

    public virtual void StartTasks() {
        startingTime = Time.time;
    }

    protected override void onAnyMarkerReached(IRouteMarker marker) {
        base.onAnyMarkerReached(marker);
        
        if (marker == destinationMarker || (destinationMarker != null && marker.GetGameObject().Equals(destinationMarker.GetGameObject()))) {
            onDestinationMarkerReached(marker);
        }
    }
    
    protected virtual void onDestinationMarkerReached(IRouteMarker marker) {
        DestinationMarkerReachedEvent?.Invoke(marker);
        destinationMarker = null;
    }
    
    public void SetDestinationMarker(IRouteMarker marker) {
        if (marker is LinkedMarker { HasLinkedMarker: true } linkedMarker) {
            marker = linkedMarker.MarkerLinked;
            ResetPreferredDirection();
        }
        destinationMarker = marker;
        Vector3 destinationPosition = marker.Position;
        if (MarkerGenerator.TraversableCenterProjectionOnNavMesh(marker.Position, out Vector3 destinationPositionOnNavmesh)) {
            destinationPosition = destinationPositionOnNavmesh;
        }
        SetDestination(destinationPosition);
    }
    
    public void SetDebugText(string text) {
        // agent.SetDebugNameplateText($"[{GetAgentHeight()}] " + text);
        agent.SetDebugNameplateText($"[{agentTypeID}] " + text);
    }

    public void SetDestination(Vector3 destination) {
        agent.SetDestination(destination);
        checkDestinationDistance = false;
        
        if (DEBUG) {
            Debug.DrawLine(this.transform.position + new Vector3(0f, 0.5f, 0f), destination + new Vector3(0f, 0.5f, 0f), Color.red, 2f);
        }
    }

    private void onWithinDestinationRange() {
        agent.navMeshAgent.ResetPath();
        _withinDestinationRangeDelegate?.Invoke();
        _withinDestinationRangeDelegate = null;
        checkDestinationDistance = false;
    }

    public void SetDestination(Vector3 destination, float stopDistance, [CanBeNull] OnWithinDestinationRange withinDestinationRangeDelegate) {
        SetDestination(destination);
        checkDestinationDistance = true;
        this.stopDistanceSqr = stopDistance * stopDistance;
        this._withinDestinationRangeDelegate = withinDestinationRangeDelegate;
    }

    public void DestroyAgent() {
        agent.DestroyAgent();
    }

    public void Stop() {
        agent.navMeshAgent.ResetPath();
    }

    public bool HasReachedDestination() {
        return this.agent.navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete;
    }

    public bool IsAgentNearDestination(float maxDistance) {
        return HasReachedDestination() ||
               this.agent.navMeshAgent.remainingDistance < maxDistance;
    }

    public float GetAgentHeight() {
        return visibilityHandler.agentTypes[agentTypeID].Value;
    }

    public bool HasPath() {
        return agent.navMeshAgent.hasPath;
    }

    public void ResetPreferredDirection() {
        PreferredDirection = Vector2.zero;
    }
    
    public bool IsGoalVisible() {
        return IsMarkerVisible(this.CurrentGoal());
    }

    public bool IsMarkerVisible(IRouteMarker marker) {
        if (marker is MonoBehaviour behaviour) {
            MeshRenderer markerRenderer = behaviour.GetComponent<MeshRenderer>();
            if (markerRenderer) {
                Bounds bounds = markerRenderer.bounds;
                Vector3[] vertices = new Vector3[8];
                vertices[0] = bounds.min;
                vertices[1] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
                vertices[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
                vertices[3] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
                vertices[4] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
                vertices[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
                vertices[6] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
                vertices[7] = bounds.max;

                foreach (Vector3 vertex in vertices) {
                    if (isPointVisible(vertex)) {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool isPointVisible(Vector3 point, float precision = 0.1f) {
        Vector3 agentEyePos = this.transform.position;
        agentEyePos.y += this.GetAgentHeight(); // Agent center is in the middle
        Vector3 displacementVector = point - agentEyePos;
        float distance = displacementVector.magnitude;
        
        bool rayHit = Physics.Raycast(agentEyePos, displacementVector, out RaycastHit hit, distance + precision, Constants.INVISIBLE_TO_AGENTS_LAYER_MASK);
        return !rayHit || (hit.point - point).sqrMagnitude < precision * precision;
    }
    
    public void OnTaskCompleted(bool goalReached) {
        GoalReachedEvent?.Invoke(false, goalReached);
    }

    public void OnAllTasksCompleted(bool finalGoalReached) {
        GoalReachedEvent?.Invoke(true, finalGoalReached);
        Die();
    }

    public void Die() {
        Destroy(this.gameObject);
    }

    private void OnDrawGizmos() {
        if (!DEBUG || !Application.isPlaying) {
            return;
        }
        
        Gizmos.color = Color.blue;
        Vector3 agentPosition = transform.position;
        Gizmos.DrawLine(agentPosition, agent.navMeshAgent.destination);
        
        // Gizmos.color = Color.green;
        // Gizmos.DrawLine(agentPosition, Goal);

        if (PreferredDirection != Vector2.zero) {
            Handles.color = Color.green;
            Handles.ArrowHandleCap(0, this.transform.position + new Vector3(0f, 1f, 0f), 
                Quaternion.LookRotation(PreferredDirection), 0.3f, EventType.Repaint);
        }
    }
}