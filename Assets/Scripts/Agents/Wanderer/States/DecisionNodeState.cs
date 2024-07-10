using System.Collections.Generic;
using UnityEngine;

namespace Agents.Wanderer.States {
    public class DecisionNodeState : AbstractWandererState {
        // Choose a random path to follow, but try to not backtrack -> Explore State
        
        // Maybe weight paths so that the agent is more likely to keep walking in the same direction
        
        private const float MIN_MARKER_DISTANCE = 5f;
        private const float MAX_MARKER_DISTANCE = 50f;
        private const float DONE_DELAY = 0.5f;
        private float doneTimer = -1f;

        public Vector3 NextDestination { private set; get; }

        protected override void EnterState() {
            //TODO: Chose at random between closest markers, preferring the ones in the looking direction
            List<IRouteMarker> markersAroundAgent = markersAwareAgent.GetMarkersAround(MAX_MARKER_DISTANCE, MIN_MARKER_DISTANCE); //TODO: What if no marker found
            Debug.Log($"Found  {markersAroundAgent.Count} markers");
            Vector3 agentPos = agentWanderer.transform.position;
            Vector3 agentForwardDirection = agentWanderer.transform.forward;
            
            float[] weights = new float[markersAroundAgent.Count];
            int i = 0;
            foreach (IRouteMarker marker in markersAroundAgent) {
                Vector3 markerPos = marker.Position;
                Vector3 directionToPos = (markerPos - agentPos).normalized;
                float angle = Vector3.Angle(agentForwardDirection, directionToPos);
                float weight = 1f / (angle + 1f); //TODO: Include distance too, maybe y distance counter more, because on different floor. Maybe go exponential?
                weights[i++] = weight;
            }

            int nextDestinationIndex = Utility.GetRandomWeightedIndex(weights);
            NextDestination = markersAroundAgent[nextDestinationIndex].Position;
            Debug.Log("Next dest: " + markersAroundAgent[nextDestinationIndex].Name + " " + (agentWanderer.transform.position - markersAroundAgent[nextDestinationIndex].Position).magnitude);
            setDoneDelayed(DONE_DELAY);
        }

        private void setDoneDelayed(float delay) {
            doneTimer = delay;
        }

        protected override void FixedDoState() {
            if (doneTimer > 0f) {
                doneTimer -= Time.fixedDeltaTime;
                if (doneTimer <= 0f) {
                    IsDone = true;
                    doneTimer = -1f;
                }
            }
        }
    }
}