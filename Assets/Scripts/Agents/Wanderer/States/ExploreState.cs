
using System.Collections.Generic;

namespace Agents.Wanderer.States {
    public class ExploreState : AbstractWandererState {
        public enum Reason {
            None,
            EnteredVCA,
            ReachedMarker,
            OverWalked
        }
        
        // Follow the path between markers
        // Reach a marker -> Decision Node State
        // Enter VCA -> Signage Discovery State
        // The agent has walked more than two times the inter-sign distance (parameter) -> Disorientation State 
        public Reason ExitReason { get; private set; }
        
        public void SetDestination(IRouteMarker destination) {
            
        }

        protected override void OnAgentEnterVisibilityArea(List<IFCSignBoard> visibleBoards, int agentTypeID) {
            this.IsDone = true;
            ExitReason = Reason.EnteredVCA;
        }

        protected override void OnMarkerReached(IRouteMarker marker) {
            this.IsDone = true;
            ExitReason = Reason.ReachedMarker;
        }

        protected override void EnterState() {
            ExitReason = Reason.None;
        }
    }
}