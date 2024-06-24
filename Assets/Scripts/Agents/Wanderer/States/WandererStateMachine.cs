using System.Collections.Generic;
using UnityEngine;

namespace Agents.Wanderer.States {
    [RequireComponent(typeof(AgentWanderer), typeof(SignboardAwareAgent))]
    public class WandererStateMachine : MonoBehaviour {
        private AgentWanderer agentWanderer;
        private SignboardAwareAgent signboardAwareAgent;

        private AbstractWandererState currentState;
        
        private IRouteMarker currentDestination;

        private readonly AbstractWandererState[] states = {
            new ExploreState(),
            new DecisionNodeState(),
            new SignageDiscoveryState(),
            new InformationGainState(),
            new ExecuteSignageState(),
            new DisorientationState(),
            new FailSafeState()
        };
        private ExploreState ExploreState => (ExploreState)states[0];
        private DecisionNodeState DecisionNodeState => (DecisionNodeState)states[1];
        private SignageDiscoveryState SignageDiscoveryState => (SignageDiscoveryState)states[2];
        private InformationGainState InformationGainState => (InformationGainState)states[3];
        private ExecuteSignageState ExecuteSignageState => (ExecuteSignageState)states[4];
        private DisorientationState DisorientationState => (DisorientationState)states[5];
        private FailSafeState FailSafeState => (FailSafeState)states[6];
        
        
        MarkerGenerator markerGen = FindObjectOfType<MarkerGenerator>();
        private RoutingGraphCPTSolver routingGraph;
        private bool ready = false;
        private void Awake() {
            agentWanderer = GetComponent<AgentWanderer>();
            if (markerGen == null) {
                Debug.LogError($"Unable to find {nameof(MarkerGenerator)}");
            }
        }

        private void Start() {
            if (markerGen.Markers != null) 
                initMarkers(markerGen.Markers);
            else
                markerGen.OnMarkersGeneration += OnMarkersGenerated;
            
            signboardAwareAgent.OnAgentEnterVisibilityArea += OnAgentEnterVisibilityArea;
            
            foreach (AbstractWandererState state in states) {
                state.Setup();
            }
        }

        private void initMarkers(IEnumerable<IRouteMarker> markers) {
            List<IRouteMarker> markersConnected = MarkerGenerator.RemoveUnreachableMarkersFromPosition(markers, this.transform.position);
            routingGraph = new RoutingGraphCPTSolver(markersConnected.ToArray());
        }

        private void OnMarkersGenerated(List<IRouteMarker> markers) {
            markerGen.OnMarkersGeneration -= OnMarkersGenerated;
            initMarkers(markers);
        }

        private void OnAgentEnterVisibilityArea(List<IFCSignBoard> visibleBoards, int agentTypeID) {
            
        }

        private void Update() {
            currentState.Do();
        }

        private void FixedUpdate() {
            if (currentState == null || currentState.isDone) {
                SelectState();
            }
            currentState?.FixedDo();
        }

        public void SetState(AbstractWandererState newState, bool forceReset = false) {
            if (currentState != newState || forceReset) {
                currentState?.Exit();
                currentState = newState;
                currentState.Initialize();
                currentState.Enter();
            }
        }

        private void SelectState() {
            switch (currentState) {
                case ExploreState _:
                    // One or more signs found -> SignageDiscoveryState
                    // Intersection -> DecisionNodeState
                    // No sign found -> DisorientationState
                    break;
                case DecisionNodeState _:
                    // Decision Executed -> ExploreState
                    break;
                case SignageDiscoveryState _:
                    // No Correct sign found -> ExploreState
                    // Correct Sign found -> InformationGainState
                    break;
                case InformationGainState _:
                    // Enough information perceived -> ExecuteSignageState
                    break;
                case ExecuteSignageState _:
                    // Goal not visible -> ExploreState
                    // Goal not visible -> SUCCESS
                    break;
                case DisorientationState _:
                    // No sign found after a long search -> FailSafeState
                    break;
                case FailSafeState _:
                    break;
                default:
                    SetState(ExploreState, true);
                    // ExploreState.SetDestination();
                    break;
            }
            
            currentState.Enter();
            setDebugText(currentState.GetType().Name);
        }
    
        private void setDebugText(string text) {
            agentWanderer.SetDebugText(text);
        }
    }
    
}