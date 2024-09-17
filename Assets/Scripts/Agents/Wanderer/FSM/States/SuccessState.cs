
namespace Agents.Wanderer.States {
    public class SuccessState : AbstractWandererState {
        
        private const float DONE_DELAY = 2f;
        protected override void EnterState() {
            // Vector3 goal = agentWanderer.Goal.GetPosition();
            SetDestinationMarker(agentWanderer.Goal);
            // if (MarkerGenerator.TraversableCenterProjectionOnNavMesh(goal,
            //         out Vector3 goalNavmeshProjection)) {
            //     agentWanderer.SetDestination(goalNavmeshProjection, );
            // }
        }

        protected override void OnReachedDestinationMarker(IRouteMarker marker) {
            //TODO: Report of path length and time
            SetDoneDelayed(DONE_DELAY);
        }
    }
}