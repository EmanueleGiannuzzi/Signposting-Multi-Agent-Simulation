﻿
using System;
using System.Collections.Generic;
using UnityEngine;

public class AgentsHandler : MonoBehaviour{
    private static readonly List<Agent> Agents = new();
    public int AgentCount => Agents.Count;
    
    
    public delegate void OnAgentsChanged(Agent agent);
    public event OnAgentsChanged OnAgentSpawnedEvent;
    public event OnAgentsChanged BeforeAgentDestroyedEvent;
    public event OnAgentsChanged OnAgentDestroyedEvent;
    
    public void OnAgentSpawned(Agent agent) {
        Agents.Add(agent);
        OnAgentSpawnedEvent?.Invoke(agent);
    }

    public void OnAgentDestroyed(Agent agent) {
        Agents.Remove(agent);
        OnAgentDestroyedEvent?.Invoke(agent);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public Agent SpawnAgent(GameObject agentPrefab, Vector3 spawnPoint, Quaternion rotation) {
        GameObject agentGameObject = Instantiate(agentPrefab, spawnPoint, rotation);
        agentGameObject.layer = Constants.AGENTS_LAYER;
        Agent agent = agentGameObject.GetComponent<Agent>();
        if (agent == null) {
            throw new ArgumentException($"Unable to find {nameof(Agent)} component in Agent Prefab");
        }
        OnAgentSpawned(agent);
        return agent;
    }

    public void DestroyAgent(Agent agent) {
        BeforeAgentDestroyedEvent?.Invoke(agent);
        Destroy(agent.gameObject);
        OnAgentDestroyed(agent);
    }
}