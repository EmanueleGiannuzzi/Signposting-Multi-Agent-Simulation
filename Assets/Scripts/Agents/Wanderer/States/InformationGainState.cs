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

        public bool IsThereAnyUnvisitedSignboard(IEnumerable<IFCSignBoard> signboards) {
            return signboards.Any(signboard => !agentWanderer.VisitedSigns.Contains(signboard));
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
            SetDone();
            this.ExitReason = Reason.NoInformationFound;
        }
        
        

        private void checkSignboard(IFCSignBoard signboard) {
            if (agentWanderer.VisitedSigns.Contains(signboard)) {
                return;
            }
            agentWanderer.VisitedSigns.Add(signboard);

            SignboardDirections signDirection = signboard.GetComponent<SignboardDirections>();
            if (signDirection) {
                if (signDirection.TryGetDirection(agentWanderer.Goal, out Vector3 nextGoal)) {
                    agentWanderer.SetDestination(nextGoal, 0.5f, SetDone);
                    ExitReason = Reason.InformationFound;
                }
            }
        }
        
        protected override void EnterState() {
            ExitReason = Reason.None;
        }
    }
}