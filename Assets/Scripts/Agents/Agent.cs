using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Agent : MonoBehaviour {
    public float AgentFOVDegrees;
    [SerializeField] private TMP_Text debugNameplate;
    
    private static AgentsHandler agentsHandler;
    private NavMeshAgent navMeshAgent;

    public Vector3 currentDestination => navMeshAgent.destination;

    private void Awake() {
        agentsHandler ??= FindObjectOfType<AgentsHandler>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        SetDebugNameplateText("");
    }
    
    public void SetDestination(Vector3 destination) {
        navMeshAgent.SetDestination(destination);
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    public void MoveToDestroyer(Vector3 destination, Collider destroyer) {
        SetDestination(destination);
        DestroyOnCollision destroyOnCollision = this.gameObject.AddComponent<DestroyOnCollision>();
        destroyOnCollision.destroyer = destroyer;
    }

    public void SetDebugNameplateText(string text) {
        debugNameplate.text = text;
    }

    public void DestroyAgent() {
        agentsHandler.DestroyAgent(this);
    }
}