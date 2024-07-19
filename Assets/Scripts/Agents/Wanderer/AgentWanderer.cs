using Agents.Wanderer.States;
using UnityEngine;

//Information gatherer class
[RequireComponent(typeof(WandererStateMachine))]
public class AgentWanderer : MarkersAwareAgent {
    public int agentTypeID { get; } = -1; //TODO

    private static VisibilityHandler visibilityHandler;
    private float agentFOV => agent.AgentFOVDegrees;

    protected override void Awake() {
        base.Awake();
        visibilityHandler ??= FindObjectOfType<VisibilityHandler>();
        if (!visibilityHandler) {
            Debug.LogError($"Unable to find {nameof(VisibilityHandler)}");
        }
    }

    private void Start() {
        int randomAgentType = Random.Range(0, visibilityHandler.agentTypes.Length);
        float agentHeight = visibilityHandler.agentTypes[randomAgentType].Value;
        agent.SetModelHeight(agentHeight);
    }
    
    public void SetDebugText(string text) {
        agent.SetDebugNameplateText(text);
    }

    public void SetDestination(Vector3 destination) {
        Debug.DrawLine(this.transform.position + new Vector3(0f, 0.5f, 0f), destination + new Vector3(0f, 0.5f, 0f), Color.red, 2f);
        agent.SetDestination(destination);
    }

    public void DestroyAgent() {
        agent.DestroyAgent();
    }

}