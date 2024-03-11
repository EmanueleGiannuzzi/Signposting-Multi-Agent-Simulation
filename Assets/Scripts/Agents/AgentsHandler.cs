﻿
using System.Collections.Generic;
using UnityEngine;

public class AgentsHandler : MonoBehaviour{
    private static readonly List<Agent> Agents = new();
    public int AgentCount => Agents.Count;
    
    public void OnAgentSpawned(Agent agent) {
        Agents.Add(agent);
    }

    public void OnAgentDestroyed(Agent agent) {
        Agents.Remove(agent);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public Agent SpawnAgent(GameObject agentPrefab, Vector3 spawnPoint, Quaternion rotation) {
        GameObject agentGameObject = Instantiate(agentPrefab, spawnPoint, rotation);
        Agent agent = agentGameObject.AddComponent<Agent>();
        OnAgentSpawned(agent);
        return agent;
    }

    public void DestroyAgent(Agent agent) {
        Destroy(agent.gameObject);
        OnAgentDestroyed(agent);
    }
}