using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AgentGoal : MonoBehaviour, IRouteMarker {
    private new Collider collider;
    
    private void Awake() {
        setup();
    }

    private void OnValidate() {
        setup();
    }

    private void setup() {
        collider = GetComponent<Collider>();
    }
    
    public string GetName() {
        return this.gameObject.name;
    }

    public Vector3 GetPosition() {
        return collider.bounds.center;
    }

    private void OnDrawGizmos() {
        Bounds bounds = collider.bounds;
        Vector3 topCenter = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        
        DebugExtension.DrawArrow(topCenter + Vector3.up*1f, Vector3.down*0.5f, Color.blue);
    }
}