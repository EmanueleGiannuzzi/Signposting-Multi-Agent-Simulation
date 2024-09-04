using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Add all visible signs to a list
// Walk towards the nearest sign
// Repeat for each new sign
// Keep the signs in the list so that the agent can ignore them

// Correct Sign is found -> Information Gain State
// None of the Signs around contains useful information -> Explore State

namespace Agents.Wanderer.States {
    public class SignageDiscoveryState : AbstractWandererState {
        
        private const float SIGN_STOPPING_DISTANCE = 2f;
        private const float DONE_DELAY = 0.5f;

        public enum Reason {
            None,
            SignFound,
            NoSignFound
        }
        public Reason ExitReason { get; private set; } = Reason.None;
        
        protected override void EnterState() {
            ExitReason = Reason.None;
            
            IEnumerable<IFCSignBoard> visibleBoards = signboardAwareAgent.visibleSigns.Subtract(agentWanderer.VisitedSigns);
            IFCSignBoard[] visibleBoardsArray = visibleBoards as IFCSignBoard[] ?? visibleBoards.ToArray();
            if (!visibleBoardsArray.Any()) {
                onNoSignFound();
                return;
            }

            float minDistanceSqr = float.PositiveInfinity;
            IFCSignBoard closestSign = null;
            foreach (IFCSignBoard signboard in visibleBoardsArray) {
                float distanceSqr = (signboard.WorldCenterPoint - signboardAwareAgent.transform.position).sqrMagnitude;
                if (distanceSqr < minDistanceSqr) {
                    minDistanceSqr = distanceSqr;
                    closestSign = signboard;
                }
            }

            if (closestSign &&
                MarkerGenerator.TraversableCenterProjectionOnNavMesh(closestSign.WorldCenterPoint, 
                    out Vector3 signboardNavmeshProjection)) {
                
                agentWanderer.SetDestination(signboardNavmeshProjection, SIGN_STOPPING_DISTANCE, onSignFound);
                agentWanderer.VisitedSigns.Add(closestSign);
            }
        }
        
        private void onSignFound() {
            ExitReason = Reason.SignFound;
            SetDoneDelayed(DONE_DELAY);
        }

        private void onNoSignFound() {
            ExitReason = Reason.NoSignFound;
            SetDoneDelayed(DONE_DELAY);
        }
    }
}