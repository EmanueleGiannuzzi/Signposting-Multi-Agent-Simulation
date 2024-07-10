using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class IntermediateMarker : MonoBehaviour, IRouteMarker {
    Vector3 IRouteMarker.Position => transform.position;
    string IRouteMarker.getName() {
        return this.name;
    }

    private new Collider collider;

    private void Start() {
        collider = GetComponent<Collider>();
        if (!collider.isTrigger) {
            throw new ArgumentException($"IntermediateMarker: {gameObject.name} has no trigger collider");
        }
    }
    
    
}
