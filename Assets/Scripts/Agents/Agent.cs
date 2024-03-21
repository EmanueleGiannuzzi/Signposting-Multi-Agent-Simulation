using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Agent : MonoBehaviour {
    private NavMeshAgent navMeshAgent;
    public float AgentFOVDegrees;

    private void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
    
    public void MoveTo(Vector3 destination) {
        navMeshAgent.SetDestination(destination);
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    public void MoveToDestroyer(Vector3 destination, Collider destroyer) {
        MoveTo(destination);
        DestroyOnCollision destroyOnCollision = this.gameObject.AddComponent<DestroyOnCollision>();
        destroyOnCollision.destroyer = destroyer;
    }
}