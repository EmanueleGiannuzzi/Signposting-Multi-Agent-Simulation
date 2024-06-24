namespace Agents.Wanderer.States {
    public class ExploreState : AbstractWandererState {
        
        // Follow the path between markers
        // Reach a marker -> Decision Node State
        // Enter VCA -> Information Gain State
        // The agent has walked more than two times the inter-sign distance (parameter) -> Disorientation State 

        public void SetDestination(IRouteMarker destination) {
            
        }

        public override void FixedDo() {
            base.FixedDo();
        }
    }
}