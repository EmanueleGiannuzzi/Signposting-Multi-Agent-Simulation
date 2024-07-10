using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Agent))]
public class SignboardAwareAgent : MonoBehaviour {
    [SerializeField] private float updateFrequencyHz = 10f;
    
    private VisibilityHandler visibilityHandler;
    private Agent agent;
    
    public delegate void OnAgentInVisibilityArea(List<IFCSignBoard> visibleBoards, int agentTypeID);
    public event OnAgentInVisibilityArea OnAgentEnterVisibilityArea;

    private void Awake() {
        visibilityHandler = FindObjectOfType<VisibilityHandler>();
        if(visibilityHandler == null) {
            throw new MissingReferenceException("No Visibility Handler found!");
        }

        agent = GetComponent<Agent>();
    }

    private void Start() {
        //TODO: Check if visibility info is available
        InvokeRepeating(nameof(simulationUpdate), 0f, 1f / updateFrequencyHz);
    }

    private void simulationUpdate() {
        for(int agentTypeID = 0; agentTypeID < visibilityHandler.agentTypes.Length; agentTypeID++) {
            List<IFCSignBoard> visibleBoards = visibilityHandler.GetSignboardsVisible(this.transform.position, agentTypeID);
            if(visibleBoards == null) {
                return;
            }
            
            visibleBoards.RemoveAll(visibleBoard => !VisibilityHandler.IsSignboardInFOV(this.transform, visibleBoard, agent.AgentFOVDegrees));
            if (visibleBoards.Count > 0) {
                OnAgentEnterVisibilityArea?.Invoke(visibleBoards, agentTypeID); //TODO: Check if it's already inside
            }
        }
    }
    
    
}