using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AgentWanderer), typeof(NavMeshAgent))]
public class AgentLogger : MonoBehaviour {
    private NavMeshAgent navmeshAgent;
    private AgentWanderer agentWanderer;
    private float timeWalking;
    private float totalPathLength;
    private Vector3 lastPosition;

    private void Awake() {
        navmeshAgent = GetComponent<NavMeshAgent>();
        agentWanderer = GetComponent<AgentWanderer>();
    }

    private void Start() {
        ResetData();
        agentWanderer.GoalReachedEvent += onGoalReached;
    }

    private void ResetData() {
        totalPathLength = 0f;
        timeWalking = 0f;
        lastPosition = transform.position;
    }

    private void FixedUpdate() {
        timeWalking += Time.fixedDeltaTime;

        if (navmeshAgent.hasPath && navmeshAgent.velocity.sqrMagnitude > 0f) {
            float sqrDistanceThisFrame = (transform.position - lastPosition).sqrMagnitude;
            totalPathLength += sqrDistanceThisFrame;
            lastPosition = transform.position;
        }
    }
    
    private void onGoalReached(bool isLastGoal) {
        //TODO
        ResetData();
    }
}
