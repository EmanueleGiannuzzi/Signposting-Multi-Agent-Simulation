using System.Collections.Generic;
using UnityEngine;

namespace Agents.Wanderer.States {
    public abstract class AbstractWandererState {
        public bool IsDone { get; protected set; }
        private bool isActive = false;

        private float startTime;
        protected float runningTime => Time.time - startTime;

        public void Setup(SignboardAwareAgent signboardAwareAgent, MarkersAwareAgent markersAwareAgent) {
            signboardAwareAgent.OnAgentEnterVisibilityArea += onAgentEnterVisibilityArea;
            markersAwareAgent.MarkerReachedEvent += onMarkerReached;
        }
        
        private void onAgentEnterVisibilityArea(List<IFCSignBoard> visibleBoards, int agentTypeID) {
            if (isActive) {
                OnAgentEnterVisibilityArea(visibleBoards, agentTypeID);
            }
        }
        
        private void onMarkerReached(IRouteMarker marker) {
            if (isActive) {
                OnMarkerReached(marker);
            }
        }

        protected virtual void OnMarkerReached(IRouteMarker marker) {}

        protected virtual void OnAgentEnterVisibilityArea(List<IFCSignBoard> visibleBoards, int agentTypeID) {}

        public void Initialize() {
            IsDone = false;
        }

        public void Enter() {
            isActive = true;
            startTime = Time.time;
            
            EnterState();
        }

        public void Do() {
            DoState();
        }

        public void FixedDo() {
            FixedDoState();
        }

        public void Exit() {
            isActive = false;
            ExitState();
        }

        protected virtual void EnterState() {}
        protected virtual void DoState() {}
        protected virtual void FixedDoState() {}
        protected virtual void ExitState() {}
    }
}