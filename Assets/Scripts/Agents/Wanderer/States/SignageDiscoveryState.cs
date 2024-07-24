using System.Collections.Generic;
using UnityEngine;

// Add all visible signs to a list
// Walk towards the nearest sign
// Repeat for each new sign
// Keep the signs in the list so that the agent can ignore them

// Correct Sign is found -> Information Gain State
// None of the Signs around contains useful information -> Explore State

namespace Agents.Wanderer.States {
    public class SignageDiscoveryState : AbstractWandererState {
        private List<IFCSignBoard> visitedSigns = new ();

        public enum Reason {
            None,
            SignFound,
            NoSignFound
        }
        public Reason ExitReason { get; private set; } = Reason.None;
        
        protected override void EnterState() {
            List<IFCSignBoard> visibleBoards = signboardAwareAgent.visibleSigns;
            if (visibleBoards.Count <= 0) {
                return;
            }

            float minDistanceSqr = float.PositiveInfinity;
            IFCSignBoard closestSign = null;
            foreach (IFCSignBoard signboard in visibleBoards) {
                float distanceSqr = (signboard.WorldCenterPoint - signboardAwareAgent.transform.position).sqrMagnitude;
                if (distanceSqr < minDistanceSqr) {
                    minDistanceSqr = distanceSqr;
                    closestSign = signboard;
                }
            }

            if (closestSign) {
                agentWanderer.SetDestination(closestSign.WorldCenterPoint, 1f, onSignboardReached);
            }
        }

        private void onSignboardReached() {
            ExitReason = Reason.NoSignFound;
            this.IsDone = true;
        }
        
        protected override void FixedDoState() {
            foreach (IFCSignBoard signboard in signboardAwareAgent.visibleSigns) {
                checkSignboard(signboard);
            }
        }

        private void checkSignboard(IFCSignBoard signboard) {
            if (visitedSigns.Contains(signboard)) {
                return;
            }
            visitedSigns.Add(signboard);

            SignboardDirections signDirection = signboard.GetComponent<SignboardDirections>();
            if (signDirection) {
                Vector3 nextGoal = signDirection.GetDirection(agentWanderer.Goal);
                agentWanderer.SetDestination(nextGoal);
                this.IsDone = true;
                ExitReason = Reason.NoSignFound;
            }
        }
    }
}