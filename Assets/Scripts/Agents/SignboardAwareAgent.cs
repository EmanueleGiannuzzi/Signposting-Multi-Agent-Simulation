using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Agent))]
public class SignboardAwareAgent : MonoBehaviour {
    [SerializeField] private float updateFrequencyHz = 10f;

    public List<IFCSignBoard>[] SignboardsAroundPerAgentType;
    public List<IFCSignBoard>[] VisibleSignsPerAgentType; 
    
    private VisibilityHandler visibilityHandler;
    private Agent agent;
    
    public delegate void OnAgentInVisibilityArea(List<IFCSignBoard> visibleBoards, IFCSignBoard signboard, int agentTypeID);
    public event OnAgentInVisibilityArea OnAgentEnterVisibilityArea;
    public event OnAgentInVisibilityArea OnAgentExitVisibilityArea;

    private void Awake() {
        visibilityHandler = FindObjectOfType<VisibilityHandler>();
        if(!visibilityHandler) {
            throw new MissingReferenceException("No Visibility Handler found!");
        }
        agent = GetComponent<Agent>();
    }

    private void Start() {
        int agentTypesCount = visibilityHandler.agentTypes.Length;
        SignboardsAroundPerAgentType = new List<IFCSignBoard>[agentTypesCount];
        VisibleSignsPerAgentType = new List<IFCSignBoard>[agentTypesCount];
        for (int i = 0; i < agentTypesCount; i++) {
            SignboardsAroundPerAgentType[i] = new List<IFCSignBoard>();
            VisibleSignsPerAgentType[i] = new List<IFCSignBoard>();
        }
        
        InvokeRepeating(nameof(checkSigns), 0f, 1f / updateFrequencyHz);
    }
    

    private void checkSigns() {
        for(int agentTypeID = 0; agentTypeID < visibilityHandler.agentTypes.Length; agentTypeID++) {
            List<IFCSignBoard> visibleBoardsThisTick = visibilityHandler.GetSignboardsVisible(this.transform.position, agentTypeID);
            if(visibleBoardsThisTick == null) {
                continue;
            }
            SignboardsAroundPerAgentType[agentTypeID].Clear();
            SignboardsAroundPerAgentType[agentTypeID].AddRange(visibleBoardsThisTick);
            
            visibleBoardsThisTick.RemoveAll(visibleBoardThisTick => !VisibilityHandler.IsSignboardInFOV(this.transform, visibleBoardThisTick, agent.AgentFOVDegrees));

            foreach (IFCSignBoard visibleBoardThisTick in visibleBoardsThisTick) {
                if (!VisibleSignsPerAgentType[agentTypeID].Contains(visibleBoardThisTick)) {
                    OnAgentEnterVisibilityArea?.Invoke(visibleBoardsThisTick, visibleBoardThisTick, agentTypeID);
                }
            }
            foreach (IFCSignBoard visibleBoardLastTick in VisibleSignsPerAgentType[agentTypeID]) {
                if (!visibleBoardsThisTick.Contains(visibleBoardLastTick)) {
                    OnAgentExitVisibilityArea?.Invoke(visibleBoardsThisTick, visibleBoardLastTick, agentTypeID);
                }
            }
            
            VisibleSignsPerAgentType[agentTypeID].Clear();
            VisibleSignsPerAgentType[agentTypeID].AddRange(visibleBoardsThisTick);
        }
    }
    
    
}