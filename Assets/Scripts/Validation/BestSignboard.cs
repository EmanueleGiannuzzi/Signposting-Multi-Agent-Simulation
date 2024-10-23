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
        for (int agentTypeID = 0; agentTypeID < agentTypeSize; agentTypeID++) {
            totalSignboardViewsPerAgentType[agentTypeID] = new Dictionary<IFCSignBoard, int>();
        }

        foreach (IFCSignBoard signboard in SignboardHelper.GetIFCSignboards()) {
            SignboardVisibility signboardVisibility = signboard.GetComponent<SignboardVisibility>();
            signboardVisibility.VisibilityPerAgentType = new float[agentTypeSize];
        }
    }

    private void onAgentSpawnedEvent(Agent agent) {
        SimulationAgent simulationAgent = agent.GetComponent<SimulationAgent>();
        if (simulationAgent) {
            simulationAgent.OnSimulationAgentsFinishedEvent += onAgentSimulationFinished;
        }
    }

    private void onAgentSimulationFinished(SimulationAgent simulationAgent, Dictionary<IFCSignBoard, int>[] signboardAgentViewsPerAgentType) {
        for (int agentTypeID = 0; agentTypeID < agentTypeSize; agentTypeID++) {
            foreach (IFCSignBoard signboard in signboardAgentViewsPerAgentType[agentTypeID].Keys) {
                int viewCount = signboardAgentViewsPerAgentType[agentTypeID][signboard];
                if (!totalSignboardViewsPerAgentType[agentTypeID].TryAdd(signboard, viewCount)) {
                    totalSignboardViewsPerAgentType[agentTypeID][signboard] += viewCount;
                }
                SignboardVisibility signboardVisibility = signboard.GetComponent<SignboardVisibility>();
                signboardVisibility.VisibilityPerAgentType[agentTypeID] += viewCount;
            }
        }
        agentsHandler.DestroyAgent(simulationAgent.GetComponent<Agent>());
    }
}