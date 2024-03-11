using JetBrains.Annotations;
using UnityEngine;

public class SpawnArea : SpawnAreaBase {

    public SpawnAreaDestination[] goals;

    [CanBeNull]
    private Agent SpawnAgentMoveTo(GameObject agentPrefab, SpawnAreaDestination goal) {
        Transform destination = goal.Destination;
        Collider destroyer = goal.Destroyer;
        return SpawnAgentMoveTo(agentPrefab, destination.position, destroyer);
    }

    [CanBeNull]
    protected Agent SpawnAgentMoveTo(GameObject agentPrefab, Vector3 destination, Collider destroyer) {
        Agent agent = SpawnAgent(agentPrefab);
        if (agent != null) {
            agent.MoveToDestroyer(destination, destroyer);
        }
        return agent;
    }

    protected override Agent SpawnAgentEvent(GameObject agentPrefab) {
        return SpawnAgentMoveTo(agentPrefab, goals[Random.Range(0, goals.Length)]);
    }
    
    protected override bool ShouldSpawnAgents() {
        return base.ShouldSpawnAgents() && goals.Length != 0;
    }
}


[System.Serializable]
public class SpawnAreaDestination {
    [SerializeField]
    private Transform destination;

    [SerializeField]
    private Collider destroyer;

    public Transform Destination => destination;
    public Collider Destroyer => destroyer;

    public SpawnAreaDestination(Transform destination, Collider destroyer) {
        this.destination = destination;
        this.destroyer = destroyer;
    }
}
