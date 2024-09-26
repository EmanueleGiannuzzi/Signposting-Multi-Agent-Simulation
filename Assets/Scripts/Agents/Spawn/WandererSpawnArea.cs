﻿using UnityEngine;
using UnityEngine.Assertions;

public class WandererSpawnArea : SpawnAreaBase {
    private static MarkerGenerator markerGen;

    public AgentGoal[] Goals;
    
    private void Awake() {
        markerGen ??= FindObjectOfType<MarkerGenerator>();
        
        if (AgentPrefab.GetComponent<AgentWanderer>() == null) {
            throw new AssertionException("Invalid Agent Prefab",
                $"Trying to Spawn non {nameof(AgentWanderer)} from {nameof(WandererSpawnArea)}");
        }
    }

    protected override Agent SpawnAgentEvent(GameObject agentPrefab) {
        Agent agent = SpawnAgent(agentPrefab);
        if (agent == null) 
            return agent;
        
        AgentWanderer agentWanderer = agent.GetComponent<AgentWanderer>();
        AgentGoal chosenGoal = Goals[Random.Range(0, Goals.Length)];
        agentWanderer.AddGoal(chosenGoal);
        agentWanderer.StartTasks();
        return agent;
    }
    
    protected override bool ShouldSpawnAgents() {
        return base.ShouldSpawnAgents() && markerGen.Ready;
    }
}