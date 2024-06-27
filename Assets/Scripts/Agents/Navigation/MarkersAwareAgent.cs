using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Agent))]
public class MarkersAwareAgent : MonoBehaviour {
    protected static MarkerGenerator markerGenerator;
    protected Agent agent;
    protected bool markersReady => markerGenerator.Ready;
    
    public delegate void OnMarkerReached([NotNull] IRouteMarker marker);
    public event OnMarkerReached MarkerReachedEvent;
    

    private void Awake() {
        markerGenerator ??= FindObjectOfType<MarkerGenerator>();
        agent = GetComponent<Agent>();
    }

    private void OnTriggerEnter(Collider other) {
        IRouteMarker reachedMarker = other.GetComponent<IRouteMarker>();
        if (reachedMarker == null) {
            return;
        }
        MarkerReachedEvent?.Invoke(reachedMarker);
    }
}
