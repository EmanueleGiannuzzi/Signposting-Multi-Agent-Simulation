using System.Linq;
using Agents;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class GoalAgentSpawnArea : SpawnAreaBase {
    private static MarkerGenerator markerGen;
    
    [SerializeField] private AgentGoal[] goals;
    [FormerlySerializedAs("GoalsToAdd")]
    [Tooltip("Set to 0 to add all of them")]
    [SerializeField] private int goalsToAdd;
    [SerializeField] private bool isGoalOrderRandom;
    
    
    private new void Awake() {
        base.Awake();
        markerGen ??= FindObjectOfType<MarkerGenerator>();
        
        if (AgentPrefab.GetComponent<IAgentWithGoal>() == null) {
            throw new AssertionException("Invalid Agent Prefab",
                $"Trying to Spawn non {nameof(IAgentWithGoal)} from {nameof(GoalAgentSpawnArea)}");
        }
    }

    public void SetParameters(AgentGoal[] _goals, int _goalsToAdd, bool _isGoalOrderRandom, float spawnRate, int maxAgentsToSpawn) {
        this.goals = _goals;
        this.goalsToAdd = _goalsToAdd;
        this.isGoalOrderRandom = _isGoalOrderRandom;
        this.SpawnRate = spawnRate;
        this.AgentsToSpawn = maxAgentsToSpawn;
    }
    
    private IRouteMarker[] SelectGoals() {
        goalsToAdd = goalsToAdd == 0 ? goals.Length : Mathf.Min(goalsToAdd, goals.Length);
        
        IRouteMarker[] goalsCopy = (IRouteMarker[])goals.Clone();
        if (isGoalOrderRandom) {
            goalsCopy.Shuffle();
        }
        return goalsCopy.Take(goalsToAdd).ToArray();
    }

    protected override Agent SpawnAgentEvent(GameObject agentPrefab) {
        Agent agent = SpawnAgent(agentPrefab);
        if (!agent) return agent;
        
        IAgentWithGoal goalAgent = agent.GetComponent<IAgentWithGoal>();
        foreach (IRouteMarker goalToAdd in SelectGoals()) {
            goalAgent.AddGoal(goalToAdd);
        }
        goalAgent.StartTasks();
        return agent;
    }
    
    protected override bool ShouldSpawnAgents() {
        return base.ShouldSpawnAgents() && markerGen.Ready;
    }
}