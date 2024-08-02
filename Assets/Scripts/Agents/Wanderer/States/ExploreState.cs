
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
        
        // Follow the path between markers
        // Reach a marker -> Decision Node State
        // Enter VCA -> Signage Discovery State
        // The agent has walked more than two times the inter-sign distance (parameter) -> Disorientation State 
        public Reason ExitReason { get; private set; } = Reason.None;

        protected override void EnterState() {
            ExitReason = Reason.None;

            if (!agentWanderer.HasDestination()) {
                if (lastDestination != Vector3.negativeInfinity) {
                    agentWanderer.SetDestination(lastDestination);
                }
                else {
                    IRouteMarker closestMarker = markersAwareAgent.GetClosestMarker();
                    if (closestMarker != null) {
                        agentWanderer.SetDestination(closestMarker.Position);
                    }
                }

                lastDestination = Vector3.negativeInfinity;
            }
        }
        
        protected override void OnAgentEnterVisibilityArea(List<IFCSignBoard> visibleBoards, int agentTypeID) {
            if (agentTypeID != agentWanderer.agentTypeID) {
                return;
            }
            
            VisibleBoards.Clear();
            VisibleBoards.AddRange(visibleBoards);
            lastDestination = agentWanderer.CurrentDestination;
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

    }
}