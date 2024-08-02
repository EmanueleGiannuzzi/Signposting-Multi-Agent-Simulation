using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Agents.Wanderer.States {
    public class InformationGainState : AbstractWandererState {
        // Lookahead Distance (LD)
        // Threshold Entropy (TE) = 0.54 bits
        
        // Look at grid cells around (Greater LD = loot at more cells)
        // Go to cell with lower entropy
        // If cell has entropy < TE -> Execute Sign State //TODO: Change to actual
        
        private const float SIGN_STOPPING_DISTANCE = 1f;
        
        public enum Reason {
            None,
            InformationFound,
            NoInformationFound
        }
        public Reason ExitReason { get; private set; } = Reason.None;

        private List<IFCSignBoard> visitedSigns = new ();

        public bool IsThereAnyUnvisitedSignboard(List<IFCSignBoard> signboards) {
            return signboards.Any(signboard => !visitedSigns.Contains(signboard));
        }
        
        protected override void FixedDoState() {
            foreach (IFCSignBoard signboard in signboardAwareAgent.visibleSigns) {
                checkSignboard(signboard);
            }

            if (agentWanderer.IsAgentNearDestination(SIGN_STOPPING_DISTANCE)) {
                onSignReached();
            }
        }

        private void onSignReached() {
            this.IsDone = true;
            this.ExitReason = Reason.NoInformationFound;
        }

        private void checkSignboard(IFCSignBoard signboard) {
            if (visitedSigns.Contains(signboard)) {
                return;
            }
            visitedSigns.Add(signboard);

            SignboardDirections signDirection = signboard.GetComponent<SignboardDirections>();
            if (signDirection) {
                Vector3 nextGoal = signDirection.GetDirection(agentWanderer.Goal);
                if (nextGoal == SignboardDirections.NO_DIRECTION) {
                    return;
                }
                agentWanderer.SetDestination(nextGoal);
                this.IsDone = true;
                ExitReason = Reason.InformationFound;
            }
        }
        
        protected override void EnterState() {
            ExitReason = Reason.None;
        }
    }
}