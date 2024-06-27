
//Information gatherer class
public class AgentWanderer : MarkersAwareAgent {
    private const int agentTypeID = 0; //TODO

    private static VisibilityHandler visibilityHandler;
    private float agentFOV => agent.AgentFOVDegrees;

    private void Awake() {
        visibilityHandler ??= FindObjectOfType<VisibilityHandler>();
    }
    
    public void SetDebugText(string text) {
        agent.SetDebugNameplateText(text);
    }

    public void DestroyAgent() {
        agent.DestroyAgent();
    }

}