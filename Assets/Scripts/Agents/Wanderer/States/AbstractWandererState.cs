﻿using System.Collections.Generic;
using UnityEngine;
using Vertx.Debugging;

namespace Agents.Wanderer.States {
    public abstract class AbstractWandererState {
        public bool IsDone { get; private set; }
        private bool isActive = false;

        private float startTime;
        protected float runningTime => Time.time - startTime;

        protected SignboardAwareAgent signboardAwareAgent;
        protected MarkersAwareAgent markersAwareAgent;
        protected AgentWanderer agentWanderer;

        private IRouteMarker destinationMarker;
        
        private float doneTimer = -1f;
        protected void SetDoneDelayed(float delay) {
            doneTimer = delay;
        }

        public void Setup(AgentWanderer agentWanderer, SignboardAwareAgent signboardAwareAgent, MarkersAwareAgent markersAwareAgent) {
            this.signboardAwareAgent = signboardAwareAgent;
            this.markersAwareAgent = markersAwareAgent;
            this.agentWanderer = agentWanderer;
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
                if (marker == destinationMarker) {
                    OnReachedDestinationMarker(marker);
                }
            }
        }

        protected virtual void OnMarkerReached(IRouteMarker marker) {}

        protected void SetDestinationMarker(IRouteMarker marker) {
            destinationMarker = marker;
            agentWanderer.SetDestination(marker.Position);
        }
        
        protected virtual void OnReachedDestinationMarker(IRouteMarker marker) {}

        protected virtual void OnAgentEnterVisibilityArea(List<IFCSignBoard> visibleBoards, int agentTypeID) {}

        public void Initialize() {
            IsDone = false;
        }

        protected void SetDone() {
            if(!isActive) return;
            this.IsDone = true;
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
            if (doneTimer > 0f) {
                doneTimer -= Time.fixedDeltaTime;
                if (doneTimer <= 0f) {
                    SetDone();
                    doneTimer = -1f;
                }
            }
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