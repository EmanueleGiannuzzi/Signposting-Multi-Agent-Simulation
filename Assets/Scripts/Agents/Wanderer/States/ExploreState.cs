
using System.Collections.Generic;
using UnityEngine;

namespace Agents.Wanderer.States {
    public class ExploreState : AbstractWandererState {
        public enum Reason {
            None,
            EnteredVCA,
            ReachedMarker,
            OverWalked //TODO
        }

        public List<IFCSignBoard> VisibleBoards = new ();
        
        // Follow the path between markers
        // Reach a marker -> Decision Node State
        // Enter VCA -> Signage Discovery State
        // The agent has walked more than two times the inter-sign distance (parameter) -> Disorientation State 
        public Reason ExitReason { get; private set; } = Reason.None;

        protected override void OnAgentEnterVisibilityArea(List<IFCSignBoard> visibleBoards, int agentTypeID) {
            if (agentTypeID != agentWanderer.agentTypeID) {
                return;
            }
            
            VisibleBoards.Clear();
            VisibleBoards.AddRange(visibleBoards);
            this.IsDone = true;
            ExitReason = Reason.EnteredVCA;
            
            Debug.Log($"Found {visibleBoards.Count} boards");
            foreach (IFCSignBoard board in visibleBoards) {
                Debug.Log(board.name);
            }
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