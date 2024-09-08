using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class IntermediateMarker : MonoBehaviour, IRouteMarker {
    private new Collider collider;
    
    protected void Start() {
        collider = GetComponent<Collider>();
        if (!collider.isTrigger) {
            throw new ArgumentException($"IntermediateMarker: {gameObject.name} has no trigger collider");
        }
    }
    
    string IRouteMarker.GetName() {
        return this.name;
    }

    Vector3 IRouteMarker.GetPosition() {
        return transform.position;
    }
    
    
}
