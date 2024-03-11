using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

[Serializable]
public class CollisionEvent : UnityEvent<NavMeshAgent, Collider> { }

[RequireComponent(typeof(Agent), typeof(NavMeshAgent))]
public class DestroyOnCollision : MonoBehaviour {
    private static AgentsHandler agentsHandler;
    private Agent agent;
    
    public CollisionEvent collisionEvent;
    public Collider destroyer;

    private void Awake() {
        agentsHandler ??= FindObjectOfType<AgentsHandler>();
    }

    private void Start() {
        collisionEvent ??= new CollisionEvent();
        agent = FindObjectOfType<Agent>();
    }
    
    private void OnTriggerEnter(Collider other) {
        NavMeshAgent navMeshAgent = this.gameObject.GetComponent<NavMeshAgent>();
        collisionEvent.Invoke(navMeshAgent, other);

        if(destroyer != null && destroyer.Equals(other)) {
            agentsHandler.DestroyAgent(this.agent);
        }

        EventAgentTriggerCollider agentTriggerCollider = other.GetComponent<EventAgentTriggerCollider>();
        if (agentTriggerCollider != null) {
            agentTriggerCollider.OnAgentCrossed(navMeshAgent);
        }
    }
}
