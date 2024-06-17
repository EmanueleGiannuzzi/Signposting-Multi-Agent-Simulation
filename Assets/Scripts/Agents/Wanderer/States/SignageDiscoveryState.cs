namespace Agents.Wanderer.States {
    public class SignageDiscoveryState : AbstractWandererState {
        //Add all visible signs to a list
        // Walk towards the nearest sign
        // Repeat for each new sign
        // Keep the signs in the list so that the agent can ignore them
        
        // Correct Sign is found -> Information Gain State
        // None of the Signs around contains useful information -> Explore State
    }
}