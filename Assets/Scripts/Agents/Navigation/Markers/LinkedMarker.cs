using UnityEngine;

namespace Agents.Navigation.Markers {
    
    public class LinkedMarker : IntermediateMarker {
        [SerializeField] private LinkedMarker _markerLinked;
        public LinkedMarker MarkerLinked => _markerLinked;
        
        private new void Start() {
            base.Start();

            if (!_markerLinked) {
                Debug.LogError($"Error on GameObject(\"{gameObject.name}\"): {nameof(LinkedMarker)} must have a {nameof(LinkedMarker)} connected");
            }
        }
    }
}
