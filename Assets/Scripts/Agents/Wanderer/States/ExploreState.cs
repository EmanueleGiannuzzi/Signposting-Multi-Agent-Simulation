
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Agents.Wanderer.States {
    public class ExploreState : AbstractWandererState {
        private const float OVERWALKING_TIME = 60f;
        private const float STAND_STILL_MAX_TICK = 100f;
        private const float MIN_MOVEMENT_PER_TICK_SQR = float.Epsilon;

        private int standStillCounter = 0;
        private Vector3 lastPosition;
        
        public enum Reason {
            None,
            EnteredNewVCA,
            ReachedMarker,
            GoalVisible,
            OverWalked //TODO
        }

        // private readonly List<IFCSignBoard> VisibleBoards = new ();
        
        public IRouteMarker NextMarker { get; set; }
        
        // Follow the path between markers
        // Reach a marker -> Decision Node State
        // Enter VCA -> Signage Discovery State
        // The agent has walked more than two times the inter-sign distance (parameter) -> Disorientation State 
        public Reason ExitReason { get; private set; } = Reason.None;

        protected override void EnterState() {
            standStillCounter = 0;
            lastPosition = agentWanderer.transform.position;
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
        }
        
        protected override void OnAgentEnterVisibilityArea(List<IFCSignBoard> visibleBoards, int agentTypeID) {
            if (agentTypeID != agentWanderer.agentTypeID) {
                return;
            }
            
            IEnumerable<IFCSignBoard> relevantSignboards = visibleBoards.Subtract(agentWanderer.VisitedSigns); //TODO: Do not subtract (or add back) when Information is found
            if (relevantSignboards.Any()) {
                onNewSignboardsFound();
            }
        }

        private void onNewSignboardsFound() {
            ExitReason = Reason.EnteredNewVCA;
            SetDone();
        }

        protected override void OnDestinationMarkerReached(IRouteMarker marker) {
            ExitReason = Reason.ReachedMarker;
            SetDone();
        }

        protected override void FixedDoState() {
            if (this.runningTime >= OVERWALKING_TIME) {
                onOverwalked();
                return;
            }

            Vector3 position = agentWanderer.transform.position;
            float sqrDistanceMovedThisTick = (lastPosition - position).sqrMagnitude;
            if (sqrDistanceMovedThisTick < MIN_MOVEMENT_PER_TICK_SQR) {
                standStillCounter++;
                if (standStillCounter > STAND_STILL_MAX_TICK) {
                    ExitReason = Reason.ReachedMarker;
                    SetDone();
                }
            }
            else {
                standStillCounter = 0;
            }
            lastPosition = position;

            if (agentWanderer.IsGoalVisible()) {
                onGoalFound();
                return;
            }
        }

        private void onGoalFound() {
            ExitReason = Reason.GoalVisible;
            SetDone();
        }

        private void onOverwalked() {
            ExitReason = Reason.OverWalked;
            SetDone();
        }
    }
}