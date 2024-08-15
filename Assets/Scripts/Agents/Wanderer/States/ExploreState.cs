
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

        public readonly List<IFCSignBoard> VisibleBoards = new ();
        private Vector3 lastDestination;
        private bool hasLastDestination = false;
        
        // Follow the path between markers
        // Reach a marker -> Decision Node State
        // Enter VCA -> Signage Discovery State
        // The agent has walked more than two times the inter-sign distance (parameter) -> Disorientation State 
        public Reason ExitReason { get; private set; } = Reason.None;

        protected override void EnterState() {
            ExitReason = Reason.None;

            if (!agentWanderer.HasDestination()) {
                if (hasLastDestination) {
                    agentWanderer.SetDestination(lastDestination);
                }
                else {
                    IRouteMarker closestMarker = markersAwareAgent.GetClosestMarker();
                    if (closestMarker != null) {
                        agentWanderer.SetDestination(closestMarker.Position);
                    }
                }
            }
            
            hasLastDestination = false;
        }

        private void setLastDestination(Vector3 lastDestination) {
            this.lastDestination = lastDestination;
            hasLastDestination = false;
            Debug.DrawLine(agentWanderer.transform.position, lastDestination, Color.cyan, 1f);
        }
        
        protected override void OnAgentEnterVisibilityArea(List<IFCSignBoard> visibleBoards, int agentTypeID) {
            if (agentTypeID != agentWanderer.agentTypeID) {
                return;
            }
            
            VisibleBoards.Clear();
            VisibleBoards.AddRange(visibleBoards);
            setLastDestination(agentWanderer.CurrentDestination);
            SetDone();
            ExitReason = Reason.EnteredVCA;
            
            // Debug.Log($"Found {visibleBoards.Count} boards");
            // foreach (IFCSignBoard board in visibleBoards) {
            //     Debug.Log(board.name);
            // }
        }

        protected override void OnMarkerReached(IRouteMarker marker) {
            SetDone();
            ExitReason = Reason.ReachedMarker;
        }

    }
}