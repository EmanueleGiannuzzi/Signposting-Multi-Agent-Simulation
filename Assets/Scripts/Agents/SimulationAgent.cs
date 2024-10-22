using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SignboardAwareAgent))]
public class SimulationAgent : MonoBehaviour {
    private VisibilityHandler visibilityHandler;
    private SignboardAwareAgent signboardAwareAgent;
    
    private Dictionary<IFCSignBoard, float>[] boardsEncountersPerAgentType;
    private Dictionary<IFCSignBoard, int>[] signboardAgentViewsPerAgentType; //[agentTypeID, signageBoardID]

    [SerializeField] private bool _isSimulationEnabled = false;
    public bool simulationEnabled {
        get => _isSimulationEnabled;
        set {
            _isSimulationEnabled = value;
            if (_isSimulationEnabled) {
                onSimulationStarted();
            }
            else {
                onSimulationStopped();
            }
        }
    }
    
    public delegate void OnSimulationAgentsFinished(SimulationAgent simulationAgent, Dictionary<IFCSignBoard, int>[] signboardAgentViewsPerAgentType);
    public event OnSimulationAgentsFinished OnSimulationAgentsFinishedEvent;
    
    private int agentTypeSize;

    private void Awake() {
        visibilityHandler = FindObjectOfType<VisibilityHandler>();
        if(!visibilityHandler) {
            throw new MissingReferenceException("No Visibility Handler found!");
        }

        signboardAwareAgent = GetComponent<SignboardAwareAgent>();
        signboardAwareAgent.OnAgentEnterVisibilityArea += onAgentEnterVisibilityArea;
        signboardAwareAgent.OnAgentExitVisibilityArea += onAgentExitVisibilityArea;
    }
    
    private void Start() {
        agentTypeSize = visibilityHandler.agentTypes.Length;
        boardsEncountersPerAgentType = new Dictionary<IFCSignBoard, float>[agentTypeSize];
        if (_isSimulationEnabled) {
            onSimulationStarted();
        }
    }
    
    private void onSimulationStarted() {
        clearData();
    }

    private void onSimulationStopped() {
        OnSimulationAgentsFinishedEvent?.Invoke(this, signboardAgentViewsPerAgentType);
    }

    private void clearData() {
        boardsEncountersPerAgentType = new Dictionary<IFCSignBoard, float>[agentTypeSize];
        signboardAgentViewsPerAgentType = new Dictionary<IFCSignBoard, int>[agentTypeSize];
        for(int i = 0; i < agentTypeSize; i++) {
            boardsEncountersPerAgentType[i] = new Dictionary<IFCSignBoard, float>();
            signboardAgentViewsPerAgentType[i] = new Dictionary<IFCSignBoard, int>();
        }
    }

    private void onAgentExitVisibilityArea(List<IFCSignBoard> visibleBoards, IFCSignBoard signboard, int agentTypeID) {
        if (boardsEncountersPerAgentType[agentTypeID].TryGetValue(signboard, out float enterTime)) {
            float exitTime = Time.time;
            float residenceTime = exitTime - enterTime;
            
            if(residenceTime >= signboard.MinimumReadingTime) {
                if(!signboardAgentViewsPerAgentType[agentTypeID].TryAdd(signboard, 1)) {
                    signboardAgentViewsPerAgentType[agentTypeID][signboard]++;
                }
            }
            boardsEncountersPerAgentType[agentTypeID].Remove(signboard);
        }
    }

    private void onAgentEnterVisibilityArea(List<IFCSignBoard> visibleBoards, IFCSignBoard signboard, int agentTypeID) {
        float enterTime = Time.time;
        boardsEncountersPerAgentType[agentTypeID].TryAdd(signboard, enterTime);
    }
}
