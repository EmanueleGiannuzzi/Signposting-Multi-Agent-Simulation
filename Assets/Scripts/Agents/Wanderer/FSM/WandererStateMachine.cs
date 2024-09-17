using System;
using System.Collections.Generic;
using UnityEngine;

namespace Agents.Wanderer.States {
    [RequireComponent(typeof(AgentWanderer), typeof(SignboardAwareAgent))]
    public class WandererStateMachine : MonoBehaviour {
        private AgentWanderer agentWanderer;
        private SignboardAwareAgent signboardAwareAgent;
        private MarkersAwareAgent markersAwareAgent;

        private AbstractWandererState currentState;
        
        private IRouteMarker currentDestination;

        private readonly AbstractWandererState[] states = {
            new ExploreState(),
            new DecisionNodeState(),
            new SignageDiscoveryState(),
            new InformationGainState(),
            new DisorientationState(),
            new FailSafeState(),
            new SuccessState()
        };
        private ExploreState ExploreState => (ExploreState)states[0];
        private DecisionNodeState DecisionNodeState => (DecisionNodeState)states[1];
        private SignageDiscoveryState SignageDiscoveryState => (SignageDiscoveryState)states[2];
        private InformationGainState InformationGainState => (InformationGainState)states[3];
        private DisorientationState DisorientationState => (DisorientationState)states[4];
        private FailSafeState FailSafeState => (FailSafeState)states[5];
        private SuccessState SuccessState => (SuccessState)states[6];
        
        private static MarkerGenerator markerGen;
        private RoutingGraphCPTSolver routingGraph; //TODO: Remove
        
        private void Awake() {
            agentWanderer = GetComponent<AgentWanderer>();
            signboardAwareAgent = GetComponent<SignboardAwareAgent>();
            markersAwareAgent = GetComponent<MarkersAwareAgent>();
            
            markerGen ??= FindObjectOfType<MarkerGenerator>();
            if (markerGen == null) {
                Debug.LogError($"Unable to find {nameof(MarkerGenerator)}");
            }
        }

        private void Start() {
            if (markerGen.Markers != null) 
                initMarkers(markerGen.Markers);
            else
                markerGen.OnMarkersGeneration += OnMarkersGenerated;
            
            foreach (AbstractWandererState state in states) {
                state.Setup(agentWanderer, signboardAwareAgent, markersAwareAgent);
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

        private void Update() {
            currentState?.Do();
        }

        private void FixedUpdate() {
            if (currentState == null || currentState.IsDone) {
                SelectState();
            }
            currentState?.FixedDo();
        }

        private void setState(AbstractWandererState newState, bool forceReset = false) {
            if (currentState == newState && !forceReset) 
                return;
            currentState?.Exit();
            currentState = newState;
            currentState.Initialize();
            currentState.Enter();
        }

        private void SelectState() {
            switch (currentState) {
                case ExploreState exploreState:
                    // One or more signs found -> SignageDiscoveryState
                    // Intersection -> DecisionNodeState
                    // No sign found -> DisorientationState
                    switch (exploreState.ExitReason) {
                        case ExploreState.Reason.EnteredVCA:
                            setState(SignageDiscoveryState);
                            break;
                        case ExploreState.Reason.ReachedMarker:
                            setState(DecisionNodeState);
                            break;
                        case ExploreState.Reason.GoalVisible:
                            setState(SuccessState);
                            return;
                        case ExploreState.Reason.OverWalked:
                            setState(DisorientationState);
                            break;
                        case ExploreState.Reason.None:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case DecisionNodeState decisionNodeState:
                    ExploreState.NextMarker = decisionNodeState.NextMarker;
                    setState(ExploreState);
                    break;
                case SignageDiscoveryState signageDiscoveryState:
                    // No Correct sign found -> ExploreState
                    // Correct Sign found -> InformationGainState
                    switch (signageDiscoveryState.ExitReason) {
                        case SignageDiscoveryState.Reason.None:
                            break;
                        case SignageDiscoveryState.Reason.SignFound:
                            InformationGainState.focusSignboard = signageDiscoveryState.focusSignboard;
                            setState(InformationGainState);
                            break;
                        case SignageDiscoveryState.Reason.NoSignFound:
                            setState(DecisionNodeState);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case InformationGainState informationGainState:
                    switch (informationGainState.ExitReason) {
                        case InformationGainState.Reason.None:
                            break;
                        case InformationGainState.Reason.InformationFound:
                            setState(DecisionNodeState);
                            break;
                        case InformationGainState.Reason.NoInformationFound:
                            setState(DecisionNodeState);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case DisorientationState _:
                    // No sign found after a long search -> FailSafeState
                    break;
                case FailSafeState _:
                    break;
                case SuccessState _:
                    Destroy(agentWanderer.gameObject);
                    break;
                default:
                    setState(ExploreState, true);
                    break;
            }
            
            setDebugText(currentState.GetType().Name);
            // Debug.Log($"New State {currentState.GetType().Name}");
        }
    
        private void setDebugText(string text) {
            agentWanderer.SetDebugText(text);
        }
    }
    
}