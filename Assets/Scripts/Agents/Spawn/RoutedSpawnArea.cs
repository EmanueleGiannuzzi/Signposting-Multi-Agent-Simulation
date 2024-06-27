using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class RoutedSpawnArea : SpawnAreaBase, IRouteMarker {
    Vector3 IRouteMarker.Position => transform.position;

    private RoutingGraphCPTSolver routingGraph;
    private Queue<IRouteMarker> route;
    
    private Random rand;
    
    private void Awake() {
        rand = new Random((uint)DateTime.Now.Millisecond);
        MarkerGenerator markerGen = FindObjectOfType<MarkerGenerator>();
        if (markerGen == null) {
            Debug.LogError($"Unable to find {nameof(MarkerGenerator)}");
            return;
        }
        markerGen.OnMarkersGeneration += OnMarkersGenerated;
    }

    private void OnMarkersGenerated(List<IRouteMarker> markers) {
        List<IRouteMarker> markersConnected = MarkerGenerator.RemoveUnreachableMarkersFromPosition(markers, ((IRouteMarker)this).Position);
        markersConnected.Insert(0,this);
        routingGraph = new RoutingGraphCPTSolver(markersConnected.ToArray());
        route = routingGraph.GetRoute(this);
        if (this.Enabled) {
            StartSpawn();
        }
    }

    public Agent SpawnRoutedAgent(GameObject agentPrefab, IEnumerable<IRouteMarker> agentRoute) {
        Agent agent = SpawnAgent(agentPrefab);
        if(agent != null)
            agent.GetComponent<AgentRouted>().SetRoute(new Queue<IRouteMarker>(agentRoute));

        return agent;
    }

    public Agent SpawnAgentWithDestination(GameObject agentPrefab, Vector3 destination) {
        Agent agent = SpawnAgent(agentPrefab);
        if(agent != null)
            agent.GetComponent<AgentRouted>().SetDestinationWithError(destination);

        return agent;
    }

    private int randomExponential(int max, float rateParameter) {
        float randomUniform = rand.NextFloat(0f, 1f);
        float expValue = -Mathf.Log(1 - randomUniform) / rateParameter;
        return Mathf.Min(Mathf.FloorToInt(expValue) + 1, max);
    }
    
    private Queue<IRouteMarker> thinPath(Queue<IRouteMarker> path) {
        Queue<IRouteMarker> newPath = new Queue<IRouteMarker>(path);
        int maxItemsToRemove = (int)Mathf.Floor(newPath.Count * 0.8f);
        int itemsToRemove = rand.NextInt(0, maxItemsToRemove);

        for (int i = 0; i < itemsToRemove; i++) {
            newPath.Dequeue();
        }
        return newPath;
    }

    [CanBeNull]
    private Agent SpawnRoutedAgent(GameObject agentPrefab) {
        if (routingGraph == null) {
            Debug.LogError("Unable to find a Routing Graph to spawn the agent");
            return null;
        }
        Agent agent = SpawnAgent(agentPrefab);
        if (agent == null) {
            return null;
        }
        
        AgentRouted agentRouted = agent.AddComponent<AgentRouted>();
        if (agentRouted != null && route != null) {
            agentRouted.SetRoute(thinPath(route));
        }
        else {
            agentsHandler.DestroyAgent(agent);
        }
        
        return agent;
    }

    protected override Agent SpawnAgentEvent(GameObject agentPrefab) {
        return SpawnRoutedAgent(agentPrefab);
    }
    
    private static void DrawLineBetweenMarkers(IRouteMarker marker1, IRouteMarker marker2) {
        Vector3 dir = marker2.Position - marker1.Position;
        Gizmos.DrawRay(marker1.Position, dir);
        Gizmos.color = Color.blue;
    }
    
    private void OnDrawGizmos() {
        if (routingGraph == null) {
            return;
        }
        foreach (var arc in routingGraph.GetArcs()) {
            DrawLineBetweenMarkers(arc.Item1, arc.Item2);
        }
    }

    protected override bool ShouldSpawnAgents() {
        return base.ShouldSpawnAgents() && routingGraph != null;
    }
}
