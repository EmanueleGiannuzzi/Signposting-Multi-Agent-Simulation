using System.Collections.Generic;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
public class WandererValidation : MonoBehaviour {
    [Header("Goals")]
    [SerializeField] private int numberOfGoalsToAdd = 0;
    [SerializeField] private bool isGoalOrderRandom;
    
    [Header("Spawn")]
    [SerializeField] private int maxAgentPerSpawnArea = 0;
    [SerializeField] private float spawnRate = 0f;
    
    [Header("Signs")]
    [SerializeField] private float percentageOfUsefulSignsMin = 0f;
    [SerializeField] private float percentageOfUsefulSignsMax = 1f;
    [SerializeField] private float percentageOfUsefulSignsIncrement = 0.05f;
    
    [Header("Output")]
    [SerializeField] private string testName = "";
    [SerializeField] private string outputFilePath = "";
    

    private GoalGenerator goalGenerator;
    private List<AgentLogger.Result> resultsPerAgent;
    private int agentsSpawned = 0;
    private int agentsDone = 0;
    private float currentPercentageOfUsefulSigns;
    private SpawnAreaBase currentSpawnArea;
    private int spawnAreaTracker = 0;

    private void Awake() {
        setAllSpawnAreaEnabled(false);
    }

    private void Start() {
        resultsPerAgent = new List<AgentLogger.Result>();
    }

    private void setAllSpawnAreaEnabled(bool enabled) {
        GoalAgentSpawnArea[] spawnAreas = getSpawnAreas();
        foreach (GoalAgentSpawnArea spawnArea in spawnAreas) {
            spawnArea.Enabled = enabled;
        }
    }
    
    private void enableCurrentSpawnArea() {
        setAllSpawnAreaEnabled(false);
        currentSpawnArea.ResetAgentsToSpawn();
        currentSpawnArea.Enabled = true;
    }

    private void nextSpawnArea() {
        if (spawnAreaTracker >= getSpawnAreas().Length) {
            Debug.Log("All Tests Done");
            return;
        }
        
        currentSpawnArea = getSpawnAreas()[spawnAreaTracker];
        spawnAreaTracker++;
        
        clearAgents();
        resetSignPercentTest();
        nextSignPercentTest();
        enableCurrentSpawnArea();
    }

    private void initGoalGenerator() {
        goalGenerator = new GoalGenerator(numberOfGoalsToAdd);
    }

    public void StartTests() {
        initGoalGenerator();
        IntermediateMarker[] goalsGenerated = goalGenerator.GenerateGoals();
        initSpawnAreas(goalsGenerated);

        nextSpawnArea();
    }

    private void startTest(float percentageOfUsefulSigns) {
        goalGenerator.AddGoalsToSigns(percentageOfUsefulSigns);
        enableCurrentSpawnArea();
    }

    private void initSpawnAreas(IntermediateMarker[] goals) {
        GoalAgentSpawnArea[] spawnAreas = getSpawnAreas();
        foreach (GoalAgentSpawnArea spawnArea in spawnAreas) {
            spawnArea.SetParameters(goals, numberOfGoalsToAdd, isGoalOrderRandom, spawnRate, maxAgentPerSpawnArea);
            spawnArea.OnAgentSpawnedEvent += onAgentSpawned;
        }
    }

    private void onAgentJourneyFinished(List<AgentLogger.Result> resultsPerGoal) {
        AgentLogger.Result agentTotalResult = new();
        bool containsFailure = false;
        foreach (AgentLogger.Result result in resultsPerGoal) {
            if (!result.wasSuccess) {
                containsFailure = true;
                break;
            }
            agentTotalResult.pathLength += result.pathLength;
            agentTotalResult.timeWalking += result.timeWalking;
        }

        if (!containsFailure) {
            resultsPerAgent.Add(agentTotalResult);
        }
        agentsDone++;
        if (agentsDone >= agentsSpawned) {
            setAllSpawnAreaEnabled(false);
            clearAgents();
            onAgentGroupFinished();
        }
    }

    private void onAgentGroupFinished() {
        exportCSV();
        resultsPerAgent.Clear();
        
        // if (currentSpawnArea.name.Contains("Wanderer") && currentPercentageOfUsefulSigns <= (percentageOfUsefulSignsMax + percentageOfUsefulSignsIncrement) ) {
        if (currentPercentageOfUsefulSigns <= percentageOfUsefulSignsMax) {
            currentPercentageOfUsefulSigns += percentageOfUsefulSignsIncrement;
            nextSignPercentTest();
        }
        else {
            Debug.Log($"Tests for {currentSpawnArea.name} Done");
            nextSpawnArea();
        }
    }

    private void clearAgents() {
        GoalAgentSpawnArea[] spawnAreas = getSpawnAreas();
        foreach (GoalAgentSpawnArea spawnArea in spawnAreas) {
            spawnArea.ClearAgents();
        }
    }

    private void resetSignPercentTest() {
        currentPercentageOfUsefulSigns = percentageOfUsefulSignsMin;
    }

    private void nextSignPercentTest() {
        Debug.Log($"Starting new test {testName} with {currentPercentageOfUsefulSigns*100}% of useful signs.");
        startTest(currentPercentageOfUsefulSigns);
    }

    private void exportCSV() {
        Directory.CreateDirectory(outputFilePath);
        string agentName = currentSpawnArea.name.Replace(" SpawnArea", "");
        string fileName = $"{testName}-{agentName}-{currentPercentageOfUsefulSigns:n2}.csv";
        string filePath = Path.Combine(outputFilePath, Path.GetFileName(fileName));
        using StreamWriter writer = new StreamWriter(filePath, false);
        CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture) {
            HasHeaderRecord = true
        };
        using CsvWriter csv = new CsvWriter(writer, config);
        csv.Context.RegisterClassMap(new AgentLoggerResultDataMap());
        
        csv.WriteRecords(resultsPerAgent);
        
        Debug.Log($"CSV Exported in \"{((FileStream)(writer.BaseStream)).Name}\"");
    }

    private void onAgentSpawned(Agent agent) {
        GameObject agentGO = agent.gameObject;
        if (!agentGO.GetComponent<AgentWanderer>() ) {
            throw new AssertionException($"{nameof(AgentWanderer)} component not found", $"Agent prefab not compatible with {nameof(AgentLogger)}");
        }

        if (!agentGO.GetComponent<AgentLogger>()) {
            AgentLogger agentLogger = agentGO.AddComponent<AgentLogger>();
            agentLogger.OnAllDataCollectedEvent += onAgentJourneyFinished;
            agentsSpawned++;
        }
    }

    private GoalAgentSpawnArea[] getSpawnAreas() {
        return this.transform.GetComponentsInChildren<GoalAgentSpawnArea>();
    }
    
    private sealed class AgentLoggerResultDataMap : ClassMap<AgentLogger.Result> {
        public AgentLoggerResultDataMap() {
            Map(m => m.timeWalking).Name("WalkingTime");
            Map(m => m.pathLength).Name("PathLength");
        }
    }
}
