using System.Collections.Generic;
using System.Linq;

// Add all visible signs to a list
// Walk towards the nearest sign
// Repeat for each new sign
// Keep the signs in the list so that the agent can ignore them

// Correct Sign is found -> Information Gain State
// None of the Signs around contains useful information -> Explore State

namespace Agents.Wanderer.States {
    public class SignageDiscoveryState : AbstractWandererState {

        public enum Reason {
            None,
            SignFound,
            NoSignFound
        }
        public Reason ExitReason { get; private set; } = Reason.None;
        
        protected override void EnterState() {
            ExitReason = Reason.None;
            
            List<IFCSignBoard> visibleBoards = signboardAwareAgent.visibleSigns;
            if (visibleBoards.Count <= 0) {
                onNoSignFound();
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
                onSignFound();
            }
        }

        private void onSignFound() {
            ExitReason = Reason.SignFound;
            SetDoneDelayed(0.5f);
        }

        private void onNoSignFound() {
            ExitReason = Reason.NoSignFound;
            SetDoneDelayed(0.5f);
        }
    }
}