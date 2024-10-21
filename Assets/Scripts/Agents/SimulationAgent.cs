using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SignboardAwareAgent))]
public class SimulationAgent : MonoBehaviour {
    
    private VisibilityHandler visibilityHandler;
    private SignboardAwareAgent signboardAwareAgent;
    
    private List<IFCSignBoard>[] signboardEncounteredPerAgentType;
    private BoardsEncounter[] boardsEncountersPerAgentType;

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
    
    private int agentTypeSize => visibilityHandler.agentTypes.Length;

    private class BoardsEncounter {
        public readonly Dictionary<IFCSignBoard, float> visibleBoards = new();//Board-First seen time(Time.time)
    }

    private void Awake() {
        visibilityHandler = FindObjectOfType<VisibilityHandler>();
        if(!visibilityHandler) {
            throw new MissingReferenceException("No Visibility Handler found!");
        }

        signboardAwareAgent = GetComponent<SignboardAwareAgent>();
        signboardAwareAgent.OnAgentEnterVisibilityArea += onAgentEnterVisibilityArea;
    }
    private void Start() {
        boardsEncountersPerAgentType = new BoardsEncounter[agentTypeSize];
        if (_isSimulationEnabled) {
            onSimulationStarted();
        }
    }

    private void onSimulationStarted() {
        signboardEncounteredPerAgentType = new List<IFCSignBoard>[agentTypeSize];
        for(int i = 0; i < agentTypeSize; i++) {
            signboardEncounteredPerAgentType[i] = new List<IFCSignBoard>();
        }
    }

    private void onSimulationStopped() {
        signboardEncounteredPerAgentType = null;
    }

    private void onAgentEnterVisibilityArea(List<IFCSignBoard> visibleBoards, int agentTypeID) {
        BoardsEncounter boardsEncounters = boardsEncountersPerAgentType[agentTypeID];
        if(boardsEncounters == null) {
            boardsEncounters = new BoardsEncounter();
        }
        float now = Time.time;

        foreach(IFCSignBoard signageBoard in visibleBoards) {
            boardsEncounters.visibleBoards.TryAdd(signageBoard, now);
        }
        List<IFCSignBoard> signBoardsToRemove = new ();
        foreach(KeyValuePair<IFCSignBoard, float> boardEncounter in boardsEncounters.visibleBoards) {
            IFCSignBoard signboard = boardEncounter.Key;
            if(!visibleBoards.Contains(signboard)) {
                signBoardsToRemove.Add(signboard);

                List<IFCSignBoard> signboardEncountered = signboardEncounteredPerAgentType[agentTypeID];
                if(!signboardEncountered.Contains(signboard)) {
                    signboardEncountered.Add(signboard);
                }
            }
        }

        foreach(IFCSignBoard signboardToRemove in signBoardsToRemove) {
            boardsEncounters.visibleBoards.Remove(signboardToRemove);
        }

        boardsEncountersPerAgentType[agentTypeID] = boardsEncounters;
    }
}
