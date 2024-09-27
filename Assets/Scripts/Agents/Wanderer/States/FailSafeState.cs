namespace Agents.Wanderer.States {
    public class FailSafeState : AbstractWandererState {
        // The agent abandons the search for its goal and continues with the next task on their task list

        protected override void EnterState() {
            agentWanderer.Stop();
            SetDoneDelayed(2f);
        }
    }
}