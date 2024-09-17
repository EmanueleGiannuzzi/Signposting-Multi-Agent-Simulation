using UnityEngine;

namespace Agents.Wanderer.States {
    public class InformationGainState : AbstractWandererState {
        // Lookahead Distance (LD)
        // Threshold Entropy (TE) = 0.54 bits
        
        // Look at grid cells around (Greater LD = loot at more cells)
        // Go to cell with lower entropy
        // If cell has entropy < TE -> Execute Sign State //TODO: Change to actual

        // private bool informationFound = false;
        
        // private const float SIGN_STOPPING_DISTANCE = 0.2f;
        private const float DONE_DELAY = 0.5f;
        
        public IFCSignBoard focusSignboard;
        
        public enum Reason {
            None,
            InformationFound,
            NoInformationFound
        }
        public Reason ExitReason { get; private set; } = Reason.None;
        
        protected override void EnterState() {
            ExitReason = Reason.None;
            // informationFound = false;
            // checkSignboards();

            if (!(focusSignboard && checkSignboard(focusSignboard))) {
                onNoInformationFound();
            }
        }
        
        // protected override void FixedDoState() {
        //     if (informationFound) return;
        //     
        //     checkSignboards();
        //     
        //     
        //     
        //     if (!informationFound && agentWanderer.IsAgentNearDestination(SIGN_STOPPING_DISTANCE)) {
        //         onNoInformationFound();
        //     }
        // }

        // private void checkSignboards() {
        //     foreach (IFCSignBoard signboard in signboardAwareAgent.visibleSigns) {
        //         if (checkSignboard(signboard)) {
        //             informationFound = true;
        //             return;
        //         }
        //     }
        // }

        private bool checkSignboard(IFCSignBoard signboard) {
            // agentWanderer.VisitedSigns.Add(signboard);
            
            if (signboard.TryGetComponent(out SignboardDirections signDirection)) {
                if (signDirection.TryGetDirection(agentWanderer.Goal, out IRouteMarker nextGoal)) {
                    Vector2 nextGoalDirection = (nextGoal.Position - agentWanderer.transform.position).normalized;
                    agentWanderer.PreferredDirection = nextGoalDirection; //TODO: if it's a stair marker reset it
                    SetDestinationMarker(nextGoal);
                    return true;
                }
            }
            else {
                Debug.Log($"{signboard.name} doesn't contain a SignboardDirections");
            }
            return false;
        }

        private void onNoInformationFound() {
            Debug.Log("NO INFO FOUND");
            SetDoneDelayed(DONE_DELAY);
            this.ExitReason = Reason.NoInformationFound;
        }

        private void onInformationFound() {
            Debug.Log("INFO FOUND");
            ExitReason = Reason.InformationFound;
            SetDoneDelayed(DONE_DELAY);
        }

        protected override void OnDestinationMarkerReached(IRouteMarker marker) {
            onInformationFound();
        }
    }
}