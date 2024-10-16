using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class IntermediateMarker : MonoBehaviour, IRouteMarker {
    
    private new Collider collider;
    
    [SerializeField] private bool _isGoal;
    public bool IsGoal {
        get => _isGoal;
        set => _isGoal = value;
    }

    private void Awake() {
        setup();
    }
    
    private void setup() {
        collider = GetComponent<Collider>();
        if (!collider.isTrigger) {
            throw new ArgumentException($"{nameof(IntermediateMarker)}: {gameObject.name} has no trigger collider");
        }
    }
    
    public string GetName() {
        return this.name;
    }

    public Vector3 GetPosition() {
        return transform.position;
    }

    public GameObject GetGameObject() {
        return this.gameObject;
    }
    
    private void OnDrawGizmos() {
        if (IsGoal) {
            Bounds bounds = collider.bounds;
            Vector3 topCenter = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        
            DebugExtension.DrawArrow(topCenter + Vector3.up*1f, Vector3.down*0.5f, Color.blue);
        }
    }
}
