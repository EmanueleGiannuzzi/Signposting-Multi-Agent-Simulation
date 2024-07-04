using UnityEngine;
using UnityEngine.Assertions;

public class WandererSpawnArea : SpawnAreaBase {
    private void Awake() {
        if (AgentPrefab.GetComponent<AgentWanderer>() == null) {
            throw new AssertionException($"Invalid Agent Prefab",
                $"Trying to Spawn non {nameof(AgentWanderer)} from {nameof(WandererSpawnArea)}");
        }
    }

    protected override Agent SpawnAgentEvent(GameObject agentPrefab) {
        Agent agent = SpawnAgent(agentPrefab);
        // if (agent != null) {
        //     AgentWanderer agentWanderer = agent.GetComponent<AgentWanderer>();
        //     agentWanderer.
        // }
        return agent;
    }
    
    protected override bool ShouldSpawnAgents() {
        return base.ShouldSpawnAgents() && true; //TODO
    }
}