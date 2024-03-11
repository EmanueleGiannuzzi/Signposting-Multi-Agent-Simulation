using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class SpawnAreaBase : MonoBehaviour {
    public GameObject AgentPrefab;
    
    public bool Enabled = true;
    [Header("Agent Spawn Settings")]
    public bool EnableSpawn = true;
    [Tooltip("Agents per second.")]
    [Range(0.0f, 100.0f)]
    public float SpawnRate;
    public bool IsSpawnRandom = true;
    
    private static readonly List<Agent> agentsSpawned = new();
    private const float SPAWNED_AGENTS_INITIAL_MIN_DISTANCE = 5f;
    private const float SPAWNED_AGENTS_MIN_DISTANCE = 1f;

    private static AgentsHandler agentsHandler;
    private void Awake() {
        agentsHandler ??= FindObjectOfType<AgentsHandler>();
    }

    private void Start() {
        if(ShouldSpawnAgents()) {
            StartSpawn();
        }
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.F)) {
            EnableSpawn = !EnableSpawn;
            if(EnableSpawn) {
                StartSpawn();
            }
            else {
                StopSpawn();
            }
        }
    }
    // ReSharper disable Unity.PerformanceAnalysis
    public void StartSpawn() {
        if(SpawnRate > 0) {
            InvokeRepeating(nameof(SpawnAgents), 0f, 1 / SpawnRate);
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void StopSpawn() {
        CancelInvoke(nameof(SpawnAgents));
    }

    private void SpawnAgents() {
        if(ShouldSpawnAgents()) {
            Agent agent = SpawnAgentEvent(AgentPrefab);
            if (agent != null) {
                agent.transform.parent = this.transform;
            }
        }
    }
    
    private static IEnumerable<Agent> GetAgentsSpawnedByThis() {
        return agentsSpawned;
    }

    private static bool isSpawnPointCloseToAgents(IEnumerable<Agent> agents, Vector3 point, float minDistance) {
        if (agents == null || !agents.Any()) {
            return false;
        }

        foreach (Agent agent in agents) {
            if (agent == null) {
                continue;
            }
            Vector3 distanceVector = point - agent.transform.position;
            if (distanceVector.sqrMagnitude < minDistance * minDistance) {
                return true;
            }
        }
        return false;
    }
    
    [CanBeNull]
    protected Agent SpawnAgent(GameObject agentPrefab) {
        return SpawnAgent(GetAgentsSpawnedByThis(), agentPrefab);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    [CanBeNull]
    private Agent SpawnAgent(IEnumerable<Agent> agents, GameObject agentPrefab) {
        Transform spawnAreaTransform = this.transform;
        Vector3 position = spawnAreaTransform.position;

        Vector3 spawnPoint;
        const int MAX_TENTATIVES = 50;
        const float ERROR = 0.2f;
        float agentsMinDistance = SPAWNED_AGENTS_INITIAL_MIN_DISTANCE;
        bool spawnPointFound;
        do {
            var tentatives = 0;
            do {
                float randXOffset = 0f;
                float randZOffset = 0f;
                if (IsSpawnRandom) {
                    randXOffset = Random.Range(-ERROR, ERROR);
                    randZOffset = Random.Range(-ERROR, ERROR);
                }
                spawnPoint = new Vector3(position.x + randXOffset, position.y, position.z + randZOffset);
                tentatives++;
                spawnPointFound = !isSpawnPointCloseToAgents(agents, spawnPoint, agentsMinDistance);
            } while (tentatives < MAX_TENTATIVES && !spawnPointFound);
            agentsMinDistance -= agentsMinDistance * 0.2f;
        } while (!spawnPointFound && agentsMinDistance > SPAWNED_AGENTS_MIN_DISTANCE);

        if (!spawnPointFound) {
            Debug.LogWarning("Unable to spawn agent. Skipping. " + agentsMinDistance);
            return null;
        }

        Agent agent = agentsHandler.SpawnAgent(agentPrefab, spawnPoint, Quaternion.identity);

        agentsSpawned.Add(agent);
        return agent;
    }

    protected abstract Agent SpawnAgentEvent(GameObject agentPrefab);

    protected virtual bool ShouldSpawnAgents() {
        return Enabled;
    }
}
