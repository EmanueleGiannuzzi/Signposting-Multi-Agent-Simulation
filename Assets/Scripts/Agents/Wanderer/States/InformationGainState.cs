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

        private bool informationFound = false;
        
        private const float SIGN_STOPPING_DISTANCE = 0.2f;
        
        public enum Reason {
            None,
            InformationFound,
            NoInformationFound
        }
        public Reason ExitReason { get; private set; } = Reason.None;
        
        protected override void EnterState() {
            ExitReason = Reason.None;
            informationFound = false;
        }
        
        protected override void FixedDoState() {
            foreach (IFCSignBoard signboard in signboardAwareAgent.visibleSigns) {
                checkSignboard(signboard);
            }

            if (!informationFound && agentWanderer.IsAgentNearDestination(SIGN_STOPPING_DISTANCE)) {
                onSignReached();
            }
        }

        private void onSignReached() {
            SetDoneDelayed(0.5f);
            this.ExitReason = Reason.NoInformationFound;
        }
        
        private void checkSignboard(IFCSignBoard signboard) {
            // if (agentWanderer.VisitedSigns.Contains(signboard)) {
            //     return;
            // }
            

            agentWanderer.VisitedSigns.Add(signboard);
            
            if (signboard.TryGetComponent(out SignboardDirections signDirection)) {
                Debug.Log("DIRECTION FOUND");
                if (signDirection.TryGetDirection(agentWanderer.Goal, out IRouteMarker nextGoal)) {
                    SetDestinationMarker(nextGoal);
                    informationFound = true;
                }
            }
            else {
                Debug.Log($"{signboard.name} doesn't contain a SignboardDirections");
            }
        }
        
        public bool IsThereAnyUnvisitedSignboard(IEnumerable<IFCSignBoard> signboards) {
            return signboards.Any(signboard => !agentWanderer.VisitedSigns.Contains(signboard));
        }

        protected override void OnReachedDestinationMarker(IRouteMarker marker) {
            ExitReason = Reason.InformationFound;
            SetDone();
        }
    }
}