namespace Agents {
    public interface IAgentWithGoal {
        public void AddGoal(IRouteMarker goal);
        public IRouteMarker CurrentGoal();
        public int GoalCount();
        public IRouteMarker RemoveGoal();
        public void ClearGoals();
        public void StartTasks();
    }
}