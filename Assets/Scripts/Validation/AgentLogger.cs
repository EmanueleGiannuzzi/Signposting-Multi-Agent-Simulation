using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AgentWanderer))]
public class AgentLogger : MonoBehaviour {
    private NavMeshAgent navmeshAgent;
    private AgentWanderer agentWanderer;
    
    private Vector3 lastPosition;
    private float lastGoalTimeWalking;
    private float lastGoalPathLengthSqr;
    private float lastGoalPathLength => Mathf.Sqrt(lastGoalPathLengthSqr);
    
    
    private List<Result> resultsPerGoal;

    public delegate void OnAllDataCollected(List<Result> resultsPerGoal);
    public event OnAllDataCollected OnAllDataCollectedEvent;

    public struct Result {
        public float timeWalking;
        public float pathLength;
        public bool wasSuccess;

        public Result(float timeWalking, float pathLength, bool wasSuccess) {
            this.timeWalking = timeWalking;
            this.pathLength = pathLength;
            this.wasSuccess = wasSuccess;
        }
    }

    private void Awake() {
        navmeshAgent = GetComponent<NavMeshAgent>();
        agentWanderer = GetComponent<AgentWanderer>();
    }

    private void Start() {
        resultsPerGoal = new List<Result>();
        ResetData(true);
        agentWanderer.GoalReachedEvent += onGoalReached;
    }

    private void ResetData(bool resetLastPosition) {
        if (resetLastPosition) {
            lastPosition = transform.position;
        }
        lastGoalTimeWalking = 0f;
        lastGoalPathLengthSqr = 0f;
    }

    private void FixedUpdate() {
        lastGoalTimeWalking += Time.fixedDeltaTime;

        if (navmeshAgent.hasPath && navmeshAgent.velocity.sqrMagnitude > 0f) {
            float sqrDistanceThisFrame = (transform.position - lastPosition).sqrMagnitude;
            lastGoalPathLengthSqr += sqrDistanceThisFrame;
            lastPosition = transform.position;
        }
    }
    
    private void onGoalReached(bool isLastGoal, bool success) {
        Result resultThisGoal = new Result(lastGoalTimeWalking, lastGoalPathLength, success);
        resultsPerGoal.Add(resultThisGoal);
        ResetData(false);
        if (isLastGoal) {
            OnAllDataCollectedEvent?.Invoke(resultsPerGoal);
        }
    }
}
