namespace Agents.Wanderer.States {
    public class FailSafeState : AbstractWandererState {
        protected override void EnterState() {
            agentWanderer.Stop();
            SetDoneDelayed(2f);
        }
    }
}