using System;
using Agents.Wanderer.States;
using UnityEngine;
using Random = UnityEngine.Random;

//Information gatherer class
[RequireComponent(typeof(WandererStateMachine))]
public class AgentWanderer : MarkersAwareAgent {
    public int agentTypeID { get; private set; } = -1;

    private static VisibilityHandler visibilityHandler;
    private float agentFOV => agent.AgentFOVDegrees;

    private bool checkDestinationDistance = false;
    private float stopDistanceSqr = 0f;
    
    public Transform Goal { get; } //TODO: Set in prefab
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
        agent.SetModelHeight(agentHeight);
    }

    private void FixedUpdate() {
        if (checkDestinationDistance) {
            if ((this.transform.position - agent.currentDestination).sqrMagnitude < stopDistanceSqr) {
                onWithinDestinationRange();
            }
        }
    }

    public void SetDebugText(string text) {
        agent.SetDebugNameplateText(text);
    }

    public void SetDestination(Vector3 destination) {
        Debug.DrawLine(this.transform.position + new Vector3(0f, 0.5f, 0f), destination + new Vector3(0f, 0.5f, 0f), Color.red, 2f);
        agent.SetDestination(destination);
        checkDestinationDistance = false;
    }

    private void onWithinDestinationRange() {
        _withinDestinationRangeDelegate?.Invoke();
        _withinDestinationRangeDelegate = null;
        checkDestinationDistance = false;
    }

    public void SetDestination(Vector3 destination, float stopDistance, OnWithinDestinationRange withinDestinationRangeDelegate) {
        SetDestination(destination);
        checkDestinationDistance = true;
        this.stopDistanceSqr = stopDistance * stopDistance;
        this._withinDestinationRangeDelegate = withinDestinationRangeDelegate;
    }

    public void DestroyAgent() {
        agent.DestroyAgent();
    }
}