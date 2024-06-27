using UnityEngine;

public class WandererSpawnArea : SpawnAreaBase {
    protected override Agent SpawnAgentEvent(GameObject agentPrefab) {
        Agent agent = SpawnAgent(agentPrefab);
        if (agent != null) {
            
        }
        return agent;
    }
    
    protected override bool ShouldSpawnAgents() {
        return base.ShouldSpawnAgents() && true;
    }
}