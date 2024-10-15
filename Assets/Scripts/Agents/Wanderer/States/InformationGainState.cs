using UnityEngine;

namespace Agents.Wanderer.States {
    public class InformationGainState : AbstractWandererState {
        private const float DONE_DELAY = 0.5f;
        private const float GIVE_UP_TIME = 15f;
        
        public IFCSignBoard focusSignboard;
        
        public enum Reason {
            None,
            InformationFound,
            NoInformationFound
        }
        public Reason ExitReason { get; private set; } = Reason.None;
        
        protected override void EnterState() {
            ExitReason = Reason.None;

            if (!(focusSignboard && checkSignboard(focusSignboard))) {
                onNoInformationFound();
            }
        }

        private bool checkSignboard(IFCSignBoard signboard) {
            if (signboard.TryGetComponent(out SignboardDirections signDirection)) {
                if (signDirection.TryGetDirection(agentWanderer.CurrentGoal(), out IRouteMarker nextGoal)) {
                    Vector2 nextGoalDirection = (nextGoal.Position - agentWanderer.transform.position).normalized;
                    agentWanderer.PreferredDirection = nextGoalDirection; 
                    agentWanderer.SetDestinationMarker(nextGoal);
                    return true;
                }
            }
            else {
                Debug.Log($"{signboard.name} doesn't contain a SignboardDirections");
            }
            return false;
        }

        private void onNoInformationFound() {
            SetDoneDelayed(DONE_DELAY);
            this.ExitReason = Reason.NoInformationFound;
        }

        private void onInformationFound() {
            ExitReason = Reason.InformationFound;
            SetDoneDelayed(DONE_DELAY);
        }

        protected override void OnDestinationMarkerReached(IRouteMarker marker) {
            onInformationFound();
        }

        protected override void FixedDoState() {
            if (runningTime > GIVE_UP_TIME) {
                this.ExitReason = Reason.NoInformationFound;
                SetDone();
            }
        }
    }
}