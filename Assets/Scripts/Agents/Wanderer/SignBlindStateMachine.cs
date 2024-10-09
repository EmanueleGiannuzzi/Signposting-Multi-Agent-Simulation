using System;
using Agents.Wanderer;
using Agents.Wanderer.States;

public class SignBlindStateMachine : WandererStateMachine {
    protected override void SelectState() {
        switch (currentState) {
            case ExploreState exploreState:
                switch (exploreState.ExitReason) {
                    case ExploreState.Reason.EnteredNewVCA:
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
                agentWanderer.OnAllTasksCompleted(false);
                break;
            case SuccessState successState:
                switch (successState.ExitReason) {
                    case SuccessState.Reason.None:
                        break;
                    case SuccessState.Reason.ReachedIntermediateGoal:
                        agentWanderer.OnTaskCompleted(true);
                        setState(ExploreState);
                        break;
                    case SuccessState.Reason.ReachedLastGoal:
                        agentWanderer.OnAllTasksCompleted(true);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            default:
                setState(ExploreState, true);
                break;
        }
    }
}