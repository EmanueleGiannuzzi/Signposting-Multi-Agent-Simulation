using System.Collections.Generic;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
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
    
    [Header("Output")]
    [SerializeField] private string testName = "";
    [SerializeField] private string outputFilePath = "";
    

    private GoalGenerator goalGenerator;
    private List<AgentLogger.Result> resultsPerAgent;
    private int agentsSpawned = 0;
    private int agentsDone = 0;

    private void Awake() {
        signboardGridGenerator = FindObjectOfType<SignboardGridGenerator>();
        if (!signboardGridGenerator) {
            throw new AssertionException("Class not found", $"Unable to find {nameof(SignboardGridGenerator)}");
        }
        setAllSpawnAreaEnabled(false);
    }

    private void Start() {
        resultsPerAgent = new List<AgentLogger.Result>();
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
    }

    private void initSpawnAreas(AgentGoal[] goals) {
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
            exportCSV();
        }
    }

    private void exportCSV() {
        string filePath = Path.Combine(outputFilePath, Path.GetFileName(testName + ".csv"));
        using StreamWriter writer = new StreamWriter(filePath, false);
        CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture) {
            HasHeaderRecord = true
        };
        using CsvWriter csv = new CsvWriter(writer, config);
        csv.Context.RegisterClassMap(new AgentLoggerResultDataMap());
        string delimiter = csv.Configuration.Delimiter;

        csv.WriteField("sep=" + csv.Configuration.Delimiter, false);
        csv.NextRecord();
        string header = $"Walking Time{delimiter}Path Length";
        csv.WriteField(header, false);
        csv.NextRecord();
        csv.WriteRecords(resultsPerAgent);
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
            Map(m => m.pathLength).Name("PathLength");
            Map(m => m.timeWalking).Name("WalkingTime");
        }
    }
}
