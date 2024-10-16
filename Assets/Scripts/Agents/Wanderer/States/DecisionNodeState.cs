using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Agents.Wanderer.States {
    public class DecisionNodeState : AbstractWandererState {
        // Choose a random path to follow, but try to not backtrack -> Explore State
        private readonly bool DEBUG = false;
        
        private const float MIN_MARKER_DISTANCE = 0.7f;
        private const float MAX_MARKER_DISTANCE = 10f;
        private const float DONE_DELAY = 0.5f;
        
        private const float ANGLE_WEIGHT = 0.55f;
        private const float DISTANCE_WEIGHT = 0.05f;
        private const float VISITS_WEIGHT = 0.3f;
        private const float MARKERS_ON_PATH_WEIGHT = 0.1f;

        private readonly Dictionary<IRouteMarker, int> visitedMarkers = new();
        public IRouteMarker NextMarker { private set; get; }

        protected override void EnterState() {
            List<IRouteMarker> markersAroundAgent = markersAwareAgent.GetMarkersAround(MAX_MARKER_DISTANCE, MIN_MARKER_DISTANCE);
            if (NextMarker != null) 
                markersAroundAgent.Remove(NextMarker);
            markersAroundAgent.RemoveAll(marker => !agentWanderer.IsMarkerVisible(marker));
            
            if (markersAroundAgent.Count <= 0) {
                SetDoneDelayed(DONE_DELAY);
                return;
            }
            
            Vector3 agentPos = agentWanderer.transform.position;
            Vector3 agentForwardDirection = agentWanderer.transform.forward;
            Vector2 referenceAngle = agentWanderer.HasPreferredDirection ? agentWanderer.PreferredDirection : agentForwardDirection;

            float[] weights = new float[markersAroundAgent.Count];
            float[] angles = new float[markersAroundAgent.Count];
            float[] distancesSqr = new float[markersAroundAgent.Count];
            // float[] distancesZSqr = new float[markersAroundAgent.Count];
            int[] numberOfVisits = new int[markersAroundAgent.Count];
            int[] numberOfMarkersOnPath = new int[markersAroundAgent.Count];
            
            for (int i = 0; i < markersAroundAgent.Count; i++) {
                IRouteMarker marker = markersAroundAgent[i];
                
                Vector3 markerPos = marker.Position;
                Vector3 displacementVector = markerPos - agentPos;
                angles[i] = Vector2.Angle(referenceAngle, displacementVector.normalized);

                distancesSqr[i] = displacementVector.sqrMagnitude;
                // float deltaX = markerPos.x - agentPos.x;
                // float deltaY = markerPos.y - agentPos.y;
                // float deltaZ = markerPos.z - agentPos.z;
                // distancesXYSqr[i] = deltaX * deltaX + deltaY * deltaY;
                // distancesZSqr[i] = Mathf.Abs(deltaZ);

                numberOfMarkersOnPath[i] = markersOnPath(agentWanderer.transform.position, markerPos);
            }
            
            float[] anglesNormalized = Utility.Normalize(angles, value => Mathf.Pow(value, 3), true);
            float[] distancesSqrNormalized = Utility.Normalize(distancesSqr, true);
            // float[] distancesZSqrNormalized = Utility.Normalize(distancesZSqr, true);
            float[] numberOfVisitsNormalized = Utility.Normalize(numberOfVisits, true);
            float[] numberOfMarkersOnPathNormalized = Utility.Normalize(numberOfMarkersOnPath, true);

            // string debugLine = "";
            for (int i = 0; i < markersAroundAgent.Count; i++) {
                weights[i] = ANGLE_WEIGHT * anglesNormalized[i] + DISTANCE_WEIGHT * distancesSqrNormalized[i] +
                             VISITS_WEIGHT * numberOfVisitsNormalized[i]
                             + MARKERS_ON_PATH_WEIGHT * numberOfMarkersOnPathNormalized[i];
                // debugLine += $"{markersAroundAgent[i].Name}={weights[i]} ";
                // debugLine += $"[{weights[i]}] = ANGLE[{angles[i]}]{ANGLE_WEIGHT * anglesNormalized[i]} - " +
                //              $"DISTANCE_XY_WEIGHT[{distancesSqr[i]}] {DISTANCE_WEIGHT * distancesSqrNormalized[i]} - " +
                //              // $"DISTANCE_Z_WEIGHT[{distancesZSqr[i]}]={DISTANCE_Z_WEIGHT * distancesZSqrNormalized[i]} - " +
                //              $"VISITS_WEIGHT[{numberOfVisits[i]}]={VISITS_WEIGHT * numberOfVisitsNormalized[i]} - " +
                //              $"MARKERS_ON_PATH_WEIGHT[{numberOfMarkersOnPath[i]}]={MARKERS_ON_PATH_WEIGHT * numberOfMarkersOnPathNormalized[i]}\n";
            }
            // Debug.Log(debugLine);
            weights = Utility.Normalize(weights);

            int nextDestinationIndex = Utility.GetRandomWeightedIndex(weights);
            NextMarker = markersAroundAgent[nextDestinationIndex];
            // Debug.Log($"Chosen marker {NextMarker.Name} with weight: {weights[nextDestinationIndex]}");
            if (!visitedMarkers.TryAdd(NextMarker, 1)) {
                visitedMarkers[NextMarker]++;
            }

            // Debug.Log("Next dest: " + markersAroundAgent[nextDestinationIndex].Name + " Distance:" + 
            //           (agentWanderer.transform.position - markersAroundAgent[nextDestinationIndex].Position).magnitude);

            if (DEBUG) {
                drawDebugLines(markersAroundAgent, weights);
            }
            
            SetDoneDelayed(DONE_DELAY);
        }

        private int markersOnPath(Vector3 agentPos, Vector3 markerPos) {
            NavMeshPath path = new NavMeshPath();
            MarkerGenerator.TraversableCenterProjectionOnNavMesh(agentPos, out Vector3 startPos);
            MarkerGenerator.TraversableCenterProjectionOnNavMesh(markerPos, out Vector3 destinationPos);
            NavMesh.CalculatePath(startPos, destinationPos, NavMesh.AllAreas, path);
            
            return countMarkersOnPath(path);
        }

        private int countMarkersOnPath(NavMeshPath path) {
            int markersOnPath = 0;
            
            bool pathExists = path.status == NavMeshPathStatus.PathComplete;
            if (pathExists) {
                Vector3 prevCorner = agentWanderer.transform.position;
                foreach (Vector3 corner in path.corners) {
                    Debug.DrawLine(prevCorner, corner, Color.blue);
                    prevCorner = corner;
                }

                HashSet<IRouteMarker> markersHit = new();
                for (int i = 1; i < path.corners.Length; i++) {
                    Vector3 displacementVector = path.corners[i - 1] - path.corners[i];
                    Vector3 directionTowardsNextCorner = displacementVector.normalized;
                    float distanceToNextCorner = displacementVector.magnitude;
            
                    if (Physics.Raycast(path.corners[i], directionTowardsNextCorner, 
                            out RaycastHit hit, distanceToNextCorner, Constants.ONLY_MARKERS_LAYER_MASK)) {
                        IRouteMarker markerHit = hit.collider.GetComponent<IRouteMarker>();
                        if (markerHit != null && markersHit.Add(markerHit)) { 
                            markersOnPath++;
                        }
                    }
                }
            }

            return markersOnPath;
        }

        private void drawDebugLines(List<IRouteMarker> markersAroundAgent, float[] weights) {
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[6];
            colorKeys[0].color = Color.gray;
            colorKeys[1].color = new Color(0.6f, 0.1f, 0.1f);
            colorKeys[2].color = Color.yellow;
            colorKeys[3].color = new Color(0.7f, 1f, 0f);
            colorKeys[4].color = Color.green;
            colorKeys[5].color = new Color(1f, 0.84f, 0f);

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[6];
            for (int i = 0; i < 6; i++) {
                colorKeys[i].time = i / 5f;
                alphaKeys[i] = new GradientAlphaKey(1.0f, i / 5f);
            }

            gradient.SetKeys(colorKeys, alphaKeys);
            
            for (int i = 0; i < markersAroundAgent.Count; i++) {
                IRouteMarker marker = markersAroundAgent[i];
                
                Debug.DrawLine(agentWanderer.transform.position, marker.Position,
                    gradient.Evaluate(weights[i]), DONE_DELAY);
            }
        }
    }
}