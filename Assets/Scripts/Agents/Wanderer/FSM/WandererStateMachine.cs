using System;
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
            new FailSafeState(),
            new SuccessState()
        };
        private ExploreState ExploreState => (ExploreState)states[0];
        private DecisionNodeState DecisionNodeState => (DecisionNodeState)states[1];
        private SignageDiscoveryState SignageDiscoveryState => (SignageDiscoveryState)states[2];
        private InformationGainState InformationGainState => (InformationGainState)states[3];
        private FailSafeState FailSafeState => (FailSafeState)states[4];
        private SuccessState SuccessState => (SuccessState)states[5];
        
        private static MarkerGenerator markerGen;
        
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
            foreach (AbstractWandererState state in states) {
                state.Setup(agentWanderer, signboardAwareAgent, markersAwareAgent);
            }
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
                    switch (exploreState.ExitReason) {
                        case ExploreState.Reason.EnteredNewVCA:
                            setState(SignageDiscoveryState);
                            break;
                        case ExploreState.Reason.ReachedMarker:
                            setState(DecisionNodeState);
                            break;
                        case ExploreState.Reason.GoalVisible:
                            setState(SuccessState);
                            break;
                        case ExploreState.Reason.OverWalked:
                            setState(FailSafeState);
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
                case FailSafeState _:
                    onAllTasksCompleted();
                    break;
                case SuccessState successState:
                    switch (successState.ExitReason) {
                        case SuccessState.Reason.None:
                            break;
                        case SuccessState.Reason.ReachedIntermediateGoal:
                            onTaskCompleted();
                            setState(ExploreState);
                            break;
                        case SuccessState.Reason.ReachedLastGoal:
                            onAllTasksCompleted();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    setState(ExploreState, true);
                    break;
            }
            
            setDebugText(currentState.GetType().Name);
            Debug.Log($"New State {currentState.GetType().Name}");
        }

        private void onTaskCompleted() {
            
        }

        private void onAllTasksCompleted() {
            agentWanderer.Die();
        }
    
        private void setDebugText(string text) {
            agentWanderer.SetDebugText(text);
        }
    }
}