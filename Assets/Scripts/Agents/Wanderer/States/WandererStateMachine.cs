
using UnityEngine;

[RequireComponent(typeof(AgentWanderer))]
public class WandererStateMachine : MonoBehaviour {
    private AbstractWandererState currentState;
    
    private AbstractWandererState[] states = {
        new ExploreState()
    };
    private ExploreState ExploreState => (ExploreState)states[0];

    private void Start() {
        foreach (AbstractWandererState state in states) {
            state.Setup();
        }
        SetState(ExploreState);
    }

    private void Update() {
        currentState.Do();
    }

    private void FixedUpdate() {
        SelectState();
        currentState.FixedDo();
    }

    public void SetState(AbstractWandererState newState, bool forceReset = false) {
        if (currentState != newState || forceReset) {
            currentState?.Exit();
            currentState = newState;
            currentState.Initialize();
            currentState.Enter();
        }
    }

    private void SelectState() {
        //TODO: Get new state
        
        
    }
    
}