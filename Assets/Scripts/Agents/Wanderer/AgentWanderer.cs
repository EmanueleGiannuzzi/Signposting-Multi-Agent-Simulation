using Agents.Wanderer.States;
using UnityEngine;

//Information gatherer class
[RequireComponent(typeof(WandererStateMachine))]
public class AgentWanderer : MarkersAwareAgent {
    private const int agentTypeID = 0; //TODO

    private static VisibilityHandler visibilityHandler;
    private float agentFOV => agent.AgentFOVDegrees;

    protected override void Awake() {
        base.Awake();
        visibilityHandler ??= FindObjectOfType<VisibilityHandler>();
        if (visibilityHandler == null) {
            Debug.LogError($"Unable to find {nameof(VisibilityHandler)}");
        }
    }
    
    public void SetDebugText(string text) {
        agent.SetDebugNameplateText(text);
    }

    public void SetDestination(Vector3 destination) {
        Debug.DrawLine(this.transform.position, destination, Color.red, 2f);
        agent.SetDestination(destination);
    }

    public void DestroyAgent() {
        agent.DestroyAgent();
    }

}