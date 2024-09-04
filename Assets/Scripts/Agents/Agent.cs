using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Agent : MonoBehaviour {
    public float AgentFOVDegrees;
    [SerializeField] private TMP_Text debugNameplate;
    
    private static AgentsHandler agentsHandler;
    [HideInInspector] public NavMeshAgent navMeshAgent;

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

    public void SetModelHeight(float height) {//TODO: Changinging scale moves the model
        float currentHeight = getModelBounds().size.y;
        float currentScaleY = transform.localScale.y;

        Vector3 scale = this.transform.localScale;
        scale.y = currentScaleY * height / currentHeight;
        this.transform.localScale = scale;
        
        //TODO: Under a certain height maybe change model to wheelchair
    }
    
    private Bounds getModelBounds() {
        Bounds total = new Bounds(transform.position, Vector3.zero);
        foreach (Collider child in GetComponentsInChildren<Collider>()) {
            total.Encapsulate(child.bounds);
        }
        return total;
    }
}