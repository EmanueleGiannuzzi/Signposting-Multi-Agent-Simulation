using System.Collections.Generic;
using UnityEngine;

namespace Agents.Wanderer.States {
    public class DecisionNodeState : AbstractWandererState {
        // Choose a random path to follow, but try to not backtrack -> Explore State
        
        // Maybe weight paths so that the agent is more likely to keep walking in the same direction
        
        private const float MIN_MARKER_DISTANCE = 1f;
        private const float MAX_MARKER_DISTANCE = 10f;
        private const float DONE_DELAY = 2.5f;
        
        private const float ANGLE_WEIGHT = 0.4f;
        private const float DISTANCE_XY_WEIGHT = 0.1f;
        private const float DISTANCE_Z_WEIGHT = 0.3f;
        private const float VISITS_WEIGHT = 0.2f;

        private readonly Dictionary<IRouteMarker, int> visitedMarkers = new();
        public IRouteMarker NextMarker { private set; get; }

        protected override void EnterState() {
            //TODO: Chose at random between closest markers, preferring the ones in the looking direction
            List<IRouteMarker> markersAroundAgent = markersAwareAgent.GetMarkersAround(MAX_MARKER_DISTANCE, MIN_MARKER_DISTANCE); //TODO: What if no marker found
            
            Debug.Log($"Found  {markersAroundAgent.Count} markers");
            Vector3 agentPos = agentWanderer.transform.position;
            Vector3 agentForwardDirection = agentWanderer.transform.forward;

            float[] weights = new float[markersAroundAgent.Count];
            float[] angles = new float[markersAroundAgent.Count];
            float[] distancesXYSqr = new float[markersAroundAgent.Count];
            float[] distancesZSqr = new float[markersAroundAgent.Count];
            int[] numberOfVisits = new int[markersAroundAgent.Count];
            
            for (int i = 0; i < markersAroundAgent.Count; i++) {
                IRouteMarker marker = markersAroundAgent[i];
                
                Vector3 markerPos = marker.Position;
                Vector3 displacementVector = markerPos - agentPos;
                angles[i] = Vector3.Angle(agentForwardDirection, displacementVector.normalized);

                float deltaX = markerPos.x - agentPos.x;
                float deltaY = markerPos.y - agentPos.y;
                float deltaZ = markerPos.z - agentPos.z;
                distancesXYSqr[i] = deltaX * deltaX + deltaY * deltaY;
                distancesZSqr[i] = Mathf.Abs(deltaZ);
            }
            angles = Utility.Normalize(angles, true);
            distancesXYSqr = Utility.Normalize(distancesXYSqr, true);
            distancesZSqr = Utility.Normalize(distancesZSqr, true);
            float[] numberOfVisitsNormalized = Utility.Normalize(numberOfVisits, true);

            for (int i = 0; i < markersAroundAgent.Count; i++) {
                weights[i] = ANGLE_WEIGHT * angles[i] + DISTANCE_XY_WEIGHT * distancesXYSqr[i] +
                             DISTANCE_Z_WEIGHT * distancesZSqr[i] + VISITS_WEIGHT * numberOfVisitsNormalized[i];
            }

            int nextDestinationIndex = Utility.GetRandomWeightedIndex(weights);
            NextMarker = markersAroundAgent[nextDestinationIndex];
            if (!visitedMarkers.TryAdd(NextMarker, 1)) {
                visitedMarkers[NextMarker]++;
            }

            Debug.Log("Next dest: " + markersAroundAgent[nextDestinationIndex].Name + " Distance:" + 
                      (agentWanderer.transform.position - markersAroundAgent[nextDestinationIndex].Position).magnitude);
    
            drawDebugLines(markersAroundAgent, weights);
            
            SetDoneDelayed(DONE_DELAY);
        }

        private void drawDebugLines(List<IRouteMarker> markersAroundAgent, float[] weights) {
            Gradient gradient = new Gradient();
            var colors = new GradientColorKey[2];
            colors[0] = new GradientColorKey(Color.red, 0f);
            colors[1] = new GradientColorKey(Color.blue, 1f);
            var alphas = new GradientAlphaKey[2];
            alphas[0] = new GradientAlphaKey(1f, 0f);
            alphas[1] = new GradientAlphaKey(1f, 1f);
            gradient.SetKeys(colors, alphas);
            
            for (int i = 0; i < markersAroundAgent.Count; i++) {
                IRouteMarker marker = markersAroundAgent[i];
                
                Debug.DrawLine(agentWanderer.transform.position, marker.Position,
                    gradient.Evaluate(weights[i]), DONE_DELAY);
            }
        }
    }
}