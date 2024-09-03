using UnityEngine;

namespace Agents.Wanderer {
    [RequireComponent(typeof(Collider))]
    public class WandererGoal : MonoBehaviour {
        private new Collider collider;

        public Vector3 Position => collider.bounds.center;

        private void Awake() {
            collider = GetComponent<Collider>();
        }

        private void OnValidate() {
            collider = GetComponent<Collider>();
        }

        private void OnDrawGizmos() {
            // TODO: Arrow pointing down
        }
    }
}