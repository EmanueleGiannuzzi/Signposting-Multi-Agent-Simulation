namespace Agents {
    public class PerfectAgent : AgentWanderer {
        public override void StartTasks() {
            SetDestinationMarker(CurrentGoal());
        }

        protected override void onDestinationMarkerReached(IRouteMarker marker) {
            base.onDestinationMarkerReached(marker);
            RemoveCurrentGoal();
            if (GoalCount() > 0) {
                SetDestinationMarker(CurrentGoal());
            }
            else {
                Utility.DestroyObject(this.gameObject);
            }
        }
    }
}