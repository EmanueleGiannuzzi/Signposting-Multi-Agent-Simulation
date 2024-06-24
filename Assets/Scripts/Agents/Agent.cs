using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Agent : MonoBehaviour {
    private NavMeshAgent navMeshAgent;
    public float AgentFOVDegrees;

    [SerializeField] private TMP_Text debugNameplate;

    private void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        SetDebugNameplateText("");
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

    public void SetDebugNameplateText(string text) {
        debugNameplate.text = text;
    }
}