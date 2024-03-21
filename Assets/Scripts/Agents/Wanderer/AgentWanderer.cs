using UnityEngine;

[RequireComponent(typeof(Agent))]
public class AgentWanderer : MonoBehaviour {
    private const float UPDATE_FREQUENCY_HZ = 5f;

    private Agent agent;
    private float fieldOfView => agent.AgentFOVDegrees;

    private void Awake() {
        agent = GetComponent<Agent>();
    }

    private void Start() {
        InvokeRepeating(nameof(LookAround),0f, 1/UPDATE_FREQUENCY_HZ);
    }

    private void LookAround() {
        
    }
}