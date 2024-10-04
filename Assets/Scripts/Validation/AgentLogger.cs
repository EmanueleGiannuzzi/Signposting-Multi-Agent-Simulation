using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AgentWanderer), typeof(NavMeshAgent))]
public class AgentLogger : MonoBehaviour {
    private NavMeshAgent navmeshAgent;
    private AgentWanderer agentWanderer;
    
    private Vector3 lastPosition;
    private float lastGoalTimeWalking;
    private float lastGoalPathLengthSqr;
    private float lastGoalPathLength => Mathf.Sqrt(lastGoalPathLengthSqr);
    
    
    private List<Result> resultsPerGoal;
    private Result totalResult;

    private struct Result {
        public float timeWalking;
        public float pathLength;

        public Result(float timeWalking, float pathLength) {
            this.timeWalking = timeWalking;
            this.pathLength = pathLength;
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
    
    private void onGoalReached(bool isLastGoal) {
        Result resultThisGoal = new Result(lastGoalTimeWalking, lastGoalPathLength);
        resultsPerGoal.Add(resultThisGoal);
        ResetData(false);
        if (isLastGoal) {
            foreach (Result result in resultsPerGoal) {
                totalResult.pathLength += result.pathLength;
                totalResult.timeWalking += result.timeWalking;
            }
            //TODO: Send to WandererValiation
        }
    }
}
