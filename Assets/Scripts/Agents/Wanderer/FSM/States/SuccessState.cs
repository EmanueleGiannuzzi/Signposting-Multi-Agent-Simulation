
namespace Agents.Wanderer.States {
    public class SuccessState : AbstractWandererState {
        
        private const float DONE_DELAY = 2f;
        
        //TODO: Support multiple goals
        
        protected override void EnterState() {
            // Vector3 goal = agentWanderer.Goal.GetPosition();
            agentWanderer.SetDestinationMarker(agentWanderer.CurrentGoal());
            // if (MarkerGenerator.TraversableCenterProjectionOnNavMesh(goal,
            //         out Vector3 goalNavmeshProjection)) {
            //     agentWanderer.SetDestination(goalNavmeshProjection, );
            // }
        }

        protected override void OnDestinationMarkerReached(IRouteMarker marker) {
            //TODO: Report of path length and time
            SetDoneDelayed(DONE_DELAY);
        }
    }
}