
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoutingGraphCPTSolver : OpenCPTSolver {
    private IRouteMarker[] VertLabels { get; }
    
    public RoutingGraphCPTSolver(IRouteMarker[] vertexLabels) : base(vertexLabels.Length) {
        VertLabels = (IRouteMarker[])vertexLabels.Clone();
        foreach (IRouteMarker vertex in vertexLabels) {
            generateEdgesFrom(vertex);
        }
    }
    
    private void generateEdgesFrom(IRouteMarker vertex1) {
        foreach (IRouteMarker vertex2 in VertLabels) {
            if (vertex1 == vertex2) {
                continue;
            }
            if (MarkerGenerator.DoesDirectPathExistsBetweenPoints(vertex1, vertex2, out float cost)) {
                addEdge(vertex1, vertex2, cost);
            }
        }
    }
    
    private void addEdge(IRouteMarker vertex1, IRouteMarker vertex2, float cost) {
        if (cost < 0) {
            throw new ArgumentOutOfRangeException($"Edge can't have negative cost [{cost}]");
        }
        addArc("", vertex1, vertex2, cost);
    }
    
    private void addArc(string label, IRouteMarker u, IRouteMarker v, float cost) {
        int uPos = findVertex(u);
        int vPos = findVertex(v);
        base.addArc(label, uPos, vPos, cost);
        // base.addArc(label, vPos, uPos, cost);
    }
    
    private int findVertex(IRouteMarker vertex) {
        for (int i = 0; i < nVertices; i++) {
            if (VertLabels[i] == vertex) {
                return i;
            }
        }
        throw new Exception($"Unable to find vertex label: {vertex}");
    }
    
    public IEnumerable<Tuple<IRouteMarker, IRouteMarker>> GetArcs() {
        List<Tuple<IRouteMarker, IRouteMarker>> arcsObjs = new();

        foreach (Arc arc in this.arcs) {
            Tuple<IRouteMarker, IRouteMarker> arcObj = new(VertLabels[arc.u], VertLabels[arc.v]);
            arcsObjs.Add(arcObj);
        }
        
        return arcsObjs;
    }
    
    public Queue<IRouteMarker> GetRoute(IRouteMarker startVertex) {
        int startVertexPos = findVertex(startVertex);
        
        Queue<IRouteMarker> openCPT = new ();
        string debug = $"route[{nVertices}]: ";
        Queue<int> openCPTVertPos = getOpenCPT(startVertexPos);
        
        foreach (int vertexPos in openCPTVertPos) {
            debug += vertexPos + " ";
            if (vertexPos < nVertices) {
                openCPT.Enqueue(VertLabels[vertexPos]);
            }
        }
        Debug.Log(debug);
        
        openCPT.Dequeue();//remove start area
        return openCPT;
    }
}