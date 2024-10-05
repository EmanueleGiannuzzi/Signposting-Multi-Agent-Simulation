using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class SpawnAreaBase : MonoBehaviour {
    protected static AgentsHandler agentsHandler;
    
    public GameObject AgentPrefab;

    [Header("Agent Spawn Settings")] [SerializeField]
    private bool _enabled = false;
    public bool Enabled {
        get => _enabled;
        set {
            _enabled = value;
            if (Enabled) {
                StartSpawn();
            }
            else {
                StopSpawn();
            }
        } 
    }
    [Tooltip("Agents per second.")]
    [Range(0.0f, 100.0f)]
    public float SpawnRate;
    public bool IsSpawnRandom = true;
    [Tooltip("0 = No Limit")] public int AgentsToSpawn;
    
    private readonly List<Agent> agentsSpawned = new();
    private const float SPAWNED_AGENTS_INITIAL_MIN_DISTANCE = 5f;
    private const float SPAWNED_AGENTS_MIN_DISTANCE = 1f;

    public delegate void OnAgentSpawned(Agent agent);
    public event OnAgentSpawned OnAgentSpawnedEvent;
    
    
    private void Start() {
        agentsHandler ??= FindObjectOfType<AgentsHandler>();
        if(ShouldSpawnAgents()) {
            StartSpawn();
        }
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.F)) {
            Enabled = !Enabled;
        }
    }
    
    public void StartSpawn() {
        if(SpawnRate > 0) {
            InvokeRepeating(nameof(SpawnAgents), 0f, 1 / SpawnRate);
        }
    }

    public void StopSpawn() {
        CancelInvoke(nameof(SpawnAgents));
    }

    private void SpawnAgents() {
        if(ShouldSpawnAgents()) {
            Agent agent = SpawnAgentEvent(AgentPrefab);
            if (agent != null) {
                agent.transform.parent = this.transform;
                OnAgentSpawnedEvent?.Invoke(agent);
            }
        }
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
        return SpawnAgent(agentsSpawned, agentPrefab);
    }

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
            int tentatives = 0;
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

        Agent agent = null;
        if (agentsHandler != null) {
            agent = agentsHandler.SpawnAgent(agentPrefab, spawnPoint, Quaternion.identity);
        }
        else {
            Debug.LogError($"Unable to spawn Agent from {GetType().Name}: Unable to find {nameof(agentsHandler)} component.");
        }
        agentsSpawned.Add(agent);
        return agent;
    }

    protected abstract Agent SpawnAgentEvent(GameObject agentPrefab);

    protected virtual bool ShouldSpawnAgents() {
        return Enabled && canSpawnMoreAgents();
    }

    private bool canSpawnMoreAgents() {
        return AgentsToSpawn <= 0 || agentsSpawned.Count < AgentsToSpawn;
    }
}
