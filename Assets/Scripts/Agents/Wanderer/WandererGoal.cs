using UnityEngine;

namespace Agents.Wanderer {
    [RequireComponent(typeof(Collider))]
    public class WandererGoal : MonoBehaviour, IRouteMarker {
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
            // TODO: Arrow pointing down on it
        }
    }
}