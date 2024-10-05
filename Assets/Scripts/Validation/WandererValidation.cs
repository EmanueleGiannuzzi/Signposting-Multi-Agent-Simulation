using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
public class WandererValidation : MonoBehaviour {
    
    private SignboardGridGenerator signboardGridGenerator;
    
    [Header("Goals")]
    [SerializeField] private string[] goalsIfcTags = {"IfcFurnishingElement"};
    [SerializeField] private int numberOfGoalsToAdd = 0;
    [SerializeField] private bool isGoalOrderRandom;
    
    [Header("Spawn")]
    [SerializeField] private int maxAgentPerSpawnArea = 0;
    [SerializeField] private float spawnRate = 0f;
    
    [Header("Signs")]
    [SerializeField] private float percentageOfUsefulSigns = 0f;

    private GoalGenerator goalGenerator;

    private void Awake() {
        signboardGridGenerator = FindObjectOfType<SignboardGridGenerator>();
        if (!signboardGridGenerator) {
            throw new AssertionException("Class not found", $"Unable to find {nameof(SignboardGridGenerator)}");
        }
        setAllSpawnAreaEnabled(false);
    }

    private void setAllSpawnAreaEnabled(bool enabled) {
        GoalAgentSpawnArea[] spawnAreas = getSpawnAreas();
        foreach (GoalAgentSpawnArea spawnArea in spawnAreas) {
            spawnArea.enabled = enabled;
        }
    }

    public void InitGoalGenerator() {
        goalGenerator = new GoalGenerator(goalsIfcTags, numberOfGoalsToAdd, percentageOfUsefulSigns);
    }

    public void StartTest() {
        AgentGoal[] goalsGenerated = goalGenerator.GenerateGoals();
        initSpawnAreas(goalsGenerated);
        setAllSpawnAreaEnabled(true);
        //TODO
    }

    private void initSpawnAreas(AgentGoal[] goals) {
        GoalAgentSpawnArea[] spawnAreas = getSpawnAreas();
        foreach (GoalAgentSpawnArea spawnArea in spawnAreas) {
            spawnArea.SetParameters(goals, numberOfGoalsToAdd, isGoalOrderRandom, spawnRate, maxAgentPerSpawnArea);
            spawnArea.OnAgentSpawnedEvent += onAgentSpawned;
        }
    }

    private void onAgentJourneyFinished(List<AgentLogger.Result> resultsPerGoal, AgentLogger.Result totalResult) {
        
    }

    private void onAgentSpawned(Agent agent) {
        GameObject agentGO = agent.gameObject;
        if (!agentGO.GetComponent<AgentWanderer>() ) {
            throw new AssertionException($"{nameof(AgentWanderer)} component not found", $"Agent prefab not compatible with {nameof(AgentLogger)}");
        }

        if (!agentGO.GetComponent<AgentLogger>()) {
            AgentLogger agentLogger = agentGO.AddComponent<AgentLogger>();
            agentLogger.OnAllDataCollectedEvent += onAgentJourneyFinished;
        }
    }

    private GoalAgentSpawnArea[] getSpawnAreas() {
        return this.transform.GetComponentsInChildren<GoalAgentSpawnArea>();
    }
}
