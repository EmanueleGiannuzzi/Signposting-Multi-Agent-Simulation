
using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
public class AnimatorWalkingSpeedSync : MonoBehaviour {
    private Animator animator;
    private NavMeshAgent navMeshAgent;

    private void Start() {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update() {
        float speed = navMeshAgent.desiredVelocity.magnitude;
        animator.speed = speed / 2f; // The walking animation plays at 2m/s at 1x speed
        animator.SetBool(0, speed > 0.2f); //"IsWalking" parameter
    }
}