
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Agents.Wanderer.States {
    public class ExploreState : AbstractWandererState {
        private const float OVERWALKING_TIME = 60f;
        
        public enum Reason {
            None,
            EnteredVCA,
            ReachedMarker,
            GoalVisible,
            OverWalked //TODO
        }

        public readonly List<IFCSignBoard> VisibleBoards = new ();
        
        public IRouteMarker NextMarker { get; set; }
        
        // Follow the path between markers
        // Reach a marker -> Decision Node State
        // Enter VCA -> Signage Discovery State
        // The agent has walked more than two times the inter-sign distance (parameter) -> Disorientation State 
        public Reason ExitReason { get; private set; } = Reason.None;

        protected override void EnterState() {
            ExitReason = Reason.None;
            if (NextMarker != null) {
                agentWanderer.SetDestinationMarker(NextMarker);
                NextMarker = null;
            }
            else if (!agentWanderer.HasPath()) {
                goToClosestMarker();
            }
        }

        private void goToClosestMarker() {
            IRouteMarker closestMarker = markersAwareAgent.GetClosestMarker();
            if (closestMarker != null) {
                agentWanderer.SetDestinationMarker(closestMarker);
            }
            Debug.Log("Going to closest marker");
        }
        
        protected override void OnAgentEnterVisibilityArea(List<IFCSignBoard> visibleBoards, int agentTypeID) {
            if (agentTypeID != agentWanderer.agentTypeID) {
                return;
            }
            
            VisibleBoards.Clear();
            VisibleBoards.AddRange(visibleBoards);
            
            IEnumerable<IFCSignBoard> relevantSignboards = visibleBoards.Subtract(agentWanderer.VisitedSigns);
            if (relevantSignboards.Any()) {
                onNewSignboardsFound();
            }
        }

        private void onNewSignboardsFound() {
            ExitReason = Reason.EnteredVCA;
            SetDone();
        }

        protected override void OnDestinationMarkerReached(IRouteMarker marker) {
            SetDone();
            ExitReason = Reason.ReachedMarker;
        }

        protected override void FixedDoState() {
            if (this.runningTime >= OVERWALKING_TIME) {
                onOverwalked();
            }
            else {
                checkGoal();
            }
        }

        private void checkGoal() {
            if (agentWanderer.IsGoalVisible()) {
                Debug.Log("DESTINATION FOUND");
                ExitReason = Reason.GoalVisible;
                SetDoneDelayed(2f);
            }
        }

        private void onOverwalked() {
            ExitReason = Reason.OverWalked;
            SetDone();
        }
    }
}