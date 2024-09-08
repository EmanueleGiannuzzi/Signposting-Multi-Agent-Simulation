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
            checkSignboards();
        }
        
        protected override void FixedDoState() {
            if (informationFound) return;
            
            checkSignboards();
            if (!informationFound && agentWanderer.IsAgentNearDestination(SIGN_STOPPING_DISTANCE)) {
                onSignReached();
            }
        }

        private void checkSignboards() {
            foreach (IFCSignBoard signboard in signboardAwareAgent.visibleSigns) {
                if (checkSignboard(signboard)) {
                    informationFound = true;
                    return;
                }
            }
        }

        private bool checkSignboard(IFCSignBoard signboard) {
            agentWanderer.VisitedSigns.Add(signboard);
            
            if (signboard.TryGetComponent(out SignboardDirections signDirection)) {
                if (signDirection.TryGetDirection(agentWanderer.Goal, out IRouteMarker nextGoal)) {
                    Vector2 nextGoalDirection = (nextGoal.Position - agentWanderer.transform.position).normalized;
                    agentWanderer.PreferredDirection = nextGoalDirection;
                    SetDestinationMarker(nextGoal);
                    return true;
                }
            }
            else {
                Debug.Log($"{signboard.name} doesn't contain a SignboardDirections");
            }
            return false;
        }
        
        public bool IsThereAnyUnvisitedSignboard(IEnumerable<IFCSignBoard> signboards) {
            return signboards.Any(signboard => !agentWanderer.VisitedSigns.Contains(signboard));
        }

        private void onSignReached() {
            Debug.Log("NO INFO FOUND");
            SetDone();
            this.ExitReason = Reason.NoInformationFound;
        }

        private void onNextDestinationReached() {
            Debug.Log("INFO FOUND");
            ExitReason = Reason.InformationFound;
            SetDone();
        }

        protected override void OnReachedDestinationMarker(IRouteMarker marker) {
            onNextDestinationReached();
        }
    }
}