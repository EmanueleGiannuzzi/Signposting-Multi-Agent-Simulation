using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalGenerator {
    // private readonly string[] goalsIfcTags;
    private readonly int numberOfGoalsToAdd;

    private List<IntermediateMarker> goalsGenerated;

    public GoalGenerator(int numberOfGoalsToAdd) {
        // this.goalsIfcTags = goalsIfcTags;
        this.numberOfGoalsToAdd = numberOfGoalsToAdd;
    }

    private static void clearAll() {    
        clearGoals();
        clearGoalsFromSigns();
    }

    private static void clearGoals() {
        foreach (IntermediateMarker marker in getGoalCandidates()) {
            marker.IsGoal = false;
        }
    }

    private static void clearGoalsFromSigns() {
        Utility.RemoveAllComponentsOfType<SignboardDirections>();
    }
    
    private List<IntermediateMarker> generateRandomGoals() {
        List<IntermediateMarker> goalsGenerated = new ();
        IntermediateMarker[] candidates = getGoalCandidates();
        candidates.Shuffle();
        int goalsToAdd = Mathf.Min(numberOfGoalsToAdd, candidates.Length);
        
        for (int i = 0; i < goalsToAdd; i++) {
            IntermediateMarker goalCandidate = candidates[i];
            goalCandidate.IsGoal = true;
            goalsGenerated.Add(goalCandidate);
        }

        return goalsGenerated;
    }

    public void AddGoalsToSigns(float percentageOfUsefulSigns) {
        clearGoalsFromSigns();
        
        foreach (IntermediateMarker goal in goalsGenerated) {
            foreach (IFCSignBoard signboard in getSignboards()) {
                if (Utility.ProbabilityCheck(percentageOfUsefulSigns)) {
                    SignboardDirections directions = signboard.gameObject.AddComponent<SignboardDirections>();
                    directions.AddDestination(goal);
                }
            }
        }
    }

    private static IFCSignBoard[] getSignboards() {
        return Object.FindObjectsOfType<IFCSignBoard>();
    }
    
    private static IntermediateMarker[] getGoalCandidates() {
        return Object.FindObjectsOfType<IntermediateMarker>();
    }

    public IntermediateMarker[] GenerateGoals() {
        clearAll();
        goalsGenerated = generateRandomGoals();
        return goalsGenerated.ToArray();
    }
}
