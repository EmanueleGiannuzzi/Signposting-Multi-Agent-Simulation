namespace Agents.Wanderer.States {
    public class ExecuteSignageState : AbstractWandererState {
        // Walk towards the sign (use sign position)
        
        // Goal is visible and agent in goal VCA -> Success
        // Goal not visible -> Explore State
        
        private const float LOOKAHEAD_DISTANCE = 5f;

        protected override void FixedDoState() {
        }
    }
}