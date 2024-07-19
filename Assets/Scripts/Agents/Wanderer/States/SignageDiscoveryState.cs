using System.Collections.Generic;

namespace Agents.Wanderer.States {
    public class SignageDiscoveryState : AbstractWandererState {
        private List<IFCSignBoard> visitedSigns = new ();

        // Add all visible signs to a list
        // Walk towards the nearest sign
        // Repeat for each new sign
        // Keep the signs in the list so that the agent can ignore them

        // Correct Sign is found -> Information Gain State
        // None of the Signs around contains useful information -> Explore State
        
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
                agentWanderer.SetDestination(closestSign.WorldCenterPoint);
            }
        }
    }
}