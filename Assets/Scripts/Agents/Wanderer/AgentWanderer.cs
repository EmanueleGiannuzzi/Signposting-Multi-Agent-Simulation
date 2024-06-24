using UnityEngine;

[RequireComponent(typeof(Agent))]
public class AgentWanderer : MonoBehaviour {
    private const float UPDATE_FREQUENCY_HZ = 5f;
    private const int agentTypeID = 0; //TODO

    private static VisibilityHandler visibilityHandler;
    private Agent agent;

    private float agentFOV => agent.AgentFOVDegrees;

    private void Awake() {
        visibilityHandler ??= FindObjectOfType<VisibilityHandler>();
        agent = GetComponent<Agent>();
    }

    // private void Start() {
        // InvokeRepeating(nameof(lookAround),0f, 1 / UPDATE_FREQUENCY_HZ);
    // }
    
    public void SetDebugText(string text) {
        agent.SetDebugNameplateText(text);
    }

    // private void lookAround() {
    //     List<IFCSignBoard> signboardsVisible = visibilityHandler.GetSignboardsVisible(this.transform.position, agentTypeID);
    //     HashSet<RoutingSignboard> visibleRoutingSignboards = new HashSet<RoutingSignboard>();
    //
    //     foreach (IFCSignBoard ifcSignboard in signboardsVisible) {
    //         RoutingSignboard routingSignboard = ifcSignboard.GetComponent<RoutingSignboard>();
    //         if (routingSignboard != null && VisibilityHandler.IsSignboardInFOV(this.transform, routingSignboard.transform, agentFOV)) {
    //             visibleRoutingSignboards.Add(routingSignboard);
    //         }
    //     }
    //
    //     Vector3 wanderingDirection = calculateWanderingDirection(visibleRoutingSignboards);
    //     wanderTowards(wanderingDirection);
    // }
    //
    // private Vector3 calculateWanderingDirection(HashSet<RoutingSignboard> visibleRoutingSignboards) {
    //     throw new System.NotImplementedException();
    // }
    //
    // private void wanderTowards(Vector3 wanderingDirection) {
    //     throw new System.NotImplementedException();
    // }
}