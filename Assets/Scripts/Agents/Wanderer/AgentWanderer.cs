using System.Collections.Generic;
using Agents.Wanderer.States;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

//Information gatherer class
[RequireComponent(typeof(WandererStateMachine))]
public class AgentWanderer : MarkersAwareAgent {
    public int agentTypeID { get; private set; } = -1;

    private static VisibilityHandler visibilityHandler;
    private float agentFOV => agent.AgentFOVDegrees;

    private bool checkDestinationDistance = false;
    private float stopDistanceSqr = 0f;

    public Vector3 Goal { get; private set; }
    public Vector3 CurrentDestination => agent.navMeshAgent.destination;
    public readonly List<IFCSignBoard> VisitedSigns = new ();
    
    public delegate void OnWithinDestinationRange();
    private OnWithinDestinationRange _withinDestinationRangeDelegate;

    protected override void Awake() {
        base.Awake();
        visibilityHandler ??= FindObjectOfType<VisibilityHandler>();
        if (!visibilityHandler) {
            Debug.LogError($"Unable to find {nameof(VisibilityHandler)}");
        }
    }

    private void Start() {
        agentTypeID = Random.Range(0, visibilityHandler.agentTypes.Length);
        float agentHeight = visibilityHandler.agentTypes[agentTypeID].Value;
        // agent.SetModelHeight(agentHeight); //TODO: Fix
    }

    private void FixedUpdate() {
        if (checkDestinationDistance) {
            if ((this.transform.position - agent.currentDestination).sqrMagnitude < stopDistanceSqr) {
                onWithinDestinationRange();
            }
        }
    }

    public void SetGoalMarker(Vector3 goal) {
        this.Goal = goal;
    }
    public void SetDebugText(string text) {
        agent.SetDebugNameplateText(text);
    }

    public void SetDestination(Vector3 destination) {
        // Debug.Log(destination);
        // Debug.DrawLine(this.transform.position + new Vector3(0f, 0.5f, 0f), destination + new Vector3(0f, 0.5f, 0f), Color.red, 2f);
        agent.SetDestination(destination);
        checkDestinationDistance = false;
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

    public bool IsAgentNearDestination(float maxDistance) {
        return this.agent.navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete ||
               this.agent.navMeshAgent.remainingDistance < maxDistance;
    }

    public float GetEyeHeight() {
        return visibilityHandler.agentTypes[agentTypeID].Value;
    }

    public bool HasPath() {
        return agent.navMeshAgent.hasPath;
    }

    private void OnDrawGizmos() {
        if (!Application.isPlaying) {
            return;
        }
        
        Gizmos.color = Color.blue;
        Vector3 agentPosition = transform.position;
        Gizmos.DrawLine(agentPosition, agent.navMeshAgent.destination);
        // Gizmos.color = Color.green;
        // Gizmos.DrawLine(agentPosition, Goal);
    }

}