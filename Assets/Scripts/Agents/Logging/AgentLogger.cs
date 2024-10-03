using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AgentWanderer), typeof(NavMeshAgent))]
public class AgentLogger : MonoBehaviour {
    private NavMeshAgent navmeshAgent;
    private AgentWanderer agentWanderer;
    private float lastTimeWalking;
    private float totalTimeWalking;
    private float totalPathLengthSqr;
    private float lastPathLengthSqr;
    private float totalPathLengt => Mathf.Sqrt(totalPathLengthSqr);
    private Vector3 lastPosition;

    private List<Tuple<float, float>> resultsPerGoal = new List<Tuple<float, float>>();

    private void Awake() {
        navmeshAgent = GetComponent<NavMeshAgent>();
        agentWanderer = GetComponent<AgentWanderer>();
    }

    private void Start() {
        totalPathLengthSqr = 0f;
        ResetData();
        agentWanderer.GoalReachedEvent += onGoalReached;
    }

    private void ResetData() {
        totalTimeWalking = 0f;
        lastPosition = transform.position;
    }

    private void FixedUpdate() {
        totalTimeWalking += Time.fixedDeltaTime;

        if (navmeshAgent.hasPath && navmeshAgent.velocity.sqrMagnitude > 0f) {
            float sqrDistanceThisFrame = (transform.position - lastPosition).sqrMagnitude;
            totalPathLengthSqr += sqrDistanceThisFrame;
            lastPosition = transform.position;
        }
    }
    
    private void onGoalReached(bool isLastGoal) {
        //TODO
        ResetData();
    }
}
