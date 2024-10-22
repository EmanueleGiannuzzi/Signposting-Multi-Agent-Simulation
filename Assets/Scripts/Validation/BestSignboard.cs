using System.Collections.Generic;
using UnityEngine;

public class BestSignboard : MonoBehaviour {
    private AgentsHandler agentsHandler;
    private VisibilityHandler visibilityHandler;

    private Dictionary<IFCSignBoard, int>[] totalSignboardViewsPerAgentType;
    private int agentTypeSize;

    private void Awake() {
        agentsHandler = FindObjectOfType<AgentsHandler>();
        if(!agentsHandler) {
            throw new MissingReferenceException($"No {nameof(AgentsHandler)} found!");
        }
        visibilityHandler = FindObjectOfType<VisibilityHandler>();
        if(!visibilityHandler) {
            throw new MissingReferenceException($"No {nameof(VisibilityHandler)} found!");
        }
    }

    private void Start() {
        agentTypeSize = visibilityHandler.agentTypes.Length;
        agentsHandler.OnAgentSpawnedEvent += onAgentSpawnedEvent;
        clearData();
    }

    private void clearData() {
        totalSignboardViewsPerAgentType = new Dictionary<IFCSignBoard, int>[agentTypeSize];
        for (int i = 0; i < agentTypeSize; i++) {
            totalSignboardViewsPerAgentType[i] = new Dictionary<IFCSignBoard, int>();
        }
    }

    private void onAgentSpawnedEvent(Agent agent) {
        SimulationAgent simulationAgent = agent.GetComponent<SimulationAgent>();
        if (simulationAgent) {
            simulationAgent.OnSimulationAgentsFinishedEvent += onAgentSimulationFinished;
        }
    }

    private void onAgentSimulationFinished(SimulationAgent simulationAgent, Dictionary<IFCSignBoard, int>[] signboardAgentViewsPerAgentType) {
        for (int i = 0; i < agentTypeSize; i++) {
            foreach (IFCSignBoard signboard in signboardAgentViewsPerAgentType[i].Keys) {
                int viewCount = signboardAgentViewsPerAgentType[i][signboard];
                if (!totalSignboardViewsPerAgentType[i].TryAdd(signboard, viewCount)) {
                    totalSignboardViewsPerAgentType[i][signboard] += viewCount;
                }
            }
        }
        agentsHandler.DestroyAgent(simulationAgent.GetComponent<Agent>());
    }
}