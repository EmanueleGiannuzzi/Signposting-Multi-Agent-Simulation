using UnityEngine;
using UnityEngine.AI;

namespace Agents.Navigation.Markers {
    
    public class LinkedMarker : IntermediateMarker {
        [SerializeField] private LinkedMarker _markerLinked;
        public LinkedMarker MarkerLinked => _markerLinked;
        public bool HasLinkedMarker => MarkerLinked != null;
        private NavMeshPath path;
        
        private new void Start() {
            base.Start();
            // if (!_markerLinked) {
            //     Debug.LogError($"Error on GameObject(\"{gameObject.name}\"): {nameof(LinkedMarker)} must have a {nameof(LinkedMarker)} connected");
            // }
        }

        public void SetLinkedMarker(LinkedMarker linkedMarker) {
            _markerLinked = linkedMarker;
            updatePath();
        }

        private void updatePath() {
            path = new NavMeshPath();
            NavMesh.CalculatePath(this.transform.position, MarkerLinked.transform.position, NavMesh.AllAreas, path);
        }

        private void OnDrawGizmos() {
            if(!MarkerLinked) return;

            if (path == null) {
                updatePath();
            }
            
            Gizmos.color = Color.blue;
            for (int i = 1; i < path.corners.Length; i++) {
                Gizmos.DrawLine(path.corners[i-1], path.corners[i]);
            }
        }
    }
}
