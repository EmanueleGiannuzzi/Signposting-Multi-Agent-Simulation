
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
        private IRouteMarker lastDestination;
        private bool hasLastDestination = false;
        
        // Follow the path between markers
        // Reach a marker -> Decision Node State
        // Enter VCA -> Signage Discovery State
        // The agent has walked more than two times the inter-sign distance (parameter) -> Disorientation State 
        public Reason ExitReason { get; private set; } = Reason.None;

        protected override void EnterState() {
            ExitReason = Reason.None;

            if (!agentWanderer.HasPath()) {
                if (hasLastDestination) {
                    SetDestinationMarker(lastDestination);
                    Debug.Log("Going to last destination");
                }
                else {
                    goToClosestMarker();
                }
            }
            hasLastDestination = false;
        }

        private void goToClosestMarker() {
            IRouteMarker closestMarker = markersAwareAgent.GetClosestMarker();
            if (closestMarker != null) {
                SetDestinationMarker(closestMarker);
            }
            Debug.Log("Going to closest marker");
        }

        private void setLastDestination(IRouteMarker lastDestination) {
            hasLastDestination = true;
            // if (lastDestination == this.lastDestination) {
            //     return;
            // }
            
            this.lastDestination = lastDestination;
            Debug.DrawLine(agentWanderer.transform.position, lastDestination.Position, Color.cyan, 0.5f);
        }
        
        protected override void OnAgentEnterVisibilityArea(List<IFCSignBoard> visibleBoards, int agentTypeID) {
            if (agentTypeID != agentWanderer.agentTypeID) {
                return;
            }
            
            VisibleBoards.Clear();
            VisibleBoards.AddRange(visibleBoards);
            setLastDestination(destinationMarker);
            SetDone();
            ExitReason = Reason.EnteredVCA;
            
            // Debug.Log($"Found {visibleBoards.Count} boards");
            // foreach (IFCSignBoard board in visibleBoards) {
            //     Debug.Log(board.name);
            // }
        }

        protected override void OnReachedDestinationMarker(IRouteMarker marker) {
            SetDone();
            ExitReason = Reason.ReachedMarker;
        }

    }
}