
namespace Agents.Wanderer.States {
    public class SuccessState : AbstractWandererState {
        
        private const float DONE_DELAY = 1f;
        
        public enum Reason {
            None,
            ReachedIntermediateGoal,
            ReachedLastGoal
        }
        public Reason ExitReason { get; private set; } = Reason.None;
        
        protected override void EnterState() {
            ExitReason = Reason.None;
            agentWanderer.SetDestinationMarker(agentWanderer.CurrentGoal());
        }

        protected override void OnDestinationMarkerReached(IRouteMarker marker) {
            onGoalReached();
        }

        private void onGoalReached() {
            //TODO: Report of path length and time
            agentWanderer.RemoveCurrentGoal();
            ExitReason = agentWanderer.GoalCount() > 0 ? Reason.ReachedIntermediateGoal : Reason.ReachedLastGoal;
            SetDoneDelayed(DONE_DELAY);
        }
    }
}