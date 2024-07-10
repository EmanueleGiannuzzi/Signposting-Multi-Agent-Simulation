using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Agent))]
public class MarkersAwareAgent : MonoBehaviour {
    private static MarkerGenerator markerGenerator;
    protected Agent agent;
    protected bool markersReady => markerGenerator.Ready;
    
    public delegate void OnMarkerReached([NotNull] IRouteMarker marker);
    public event OnMarkerReached MarkerReachedEvent;
    

    protected virtual void Awake() {
        markerGenerator ??= FindObjectOfType<MarkerGenerator>();
        if (markerGenerator == null) {
            Debug.LogError($"Unable to find {nameof(MarkerGenerator)}");
        }
        agent = GetComponent<Agent>();
    }

    private void OnTriggerEnter(Collider other) {
        IRouteMarker reachedMarker = other.GetComponent<IRouteMarker>();
        if (reachedMarker == null) {
            return;
        }
        MarkerReachedEvent?.Invoke(reachedMarker);
    }

    public List<IRouteMarker> GetMarkersAround(float maxDistance, float minDistance = 0f) {
        float maxDistanceSqr = maxDistance * maxDistance;
        float minDistanceSqr = minDistance * minDistance;
        List<IRouteMarker> markersAroundAgent = new List<IRouteMarker>();
        foreach (IRouteMarker marker in markerGenerator.Markers) {
            float distanceSqr = (marker.Position - this.transform.position).sqrMagnitude;
            if (distanceSqr <= maxDistanceSqr && distanceSqr >= minDistanceSqr) {
                markersAroundAgent.Add(marker);
            }
        }
        
        return markersAroundAgent;
    }
}
