using System.Linq;
using Agents;
using UnityEngine;
using UnityEngine.Assertions;

public class GoalAgentSpawnArea : SpawnAreaBase {
    private static MarkerGenerator markerGen;
    
    private IRouteMarker[] Goals;
    [Tooltip("Set to 0 to add all of them")]
    [SerializeField] private int GoalsToAdd;
    [SerializeField] private bool GoalOrderRandom;
    
    
    private void Awake() {
        markerGen ??= FindObjectOfType<MarkerGenerator>();
        
        if (AgentPrefab.GetComponent<IAgentWithGoal>() == null) {
            throw new AssertionException("Invalid Agent Prefab",
                $"Trying to Spawn non {nameof(IAgentWithGoal)} from {nameof(GoalAgentSpawnArea)}");
        }
    }
    
    private IRouteMarker[] SelectGoals() {
        GoalsToAdd = Mathf.Min(GoalsToAdd, Goals.Length);
        IRouteMarker[] goalsCopy = (IRouteMarker[])Goals.Clone();

        if (GoalOrderRandom) {
            goalsCopy.Shuffle();
        }
        return goalsCopy.Take(GoalsToAdd).ToArray();
    }

    protected override Agent SpawnAgentEvent(GameObject agentPrefab) {
        Agent agent = SpawnAgent(agentPrefab);
        if (agent == null) 
            return agent;
        
        IAgentWithGoal goalAgent = agent.GetComponent<IAgentWithGoal>();
        foreach (IRouteMarker goalToAdd in SelectGoals()) {
            goalAgent.AddGoal(goalToAdd);
        }
        return agent;
    }
    
    protected override bool ShouldSpawnAgents() {
        return base.ShouldSpawnAgents() && markerGen.Ready;
    }
}