using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalGenerator {
    // private readonly string[] goalsIfcTags;
    private readonly int numberOfGoalsToAdd;

    private List<AgentGoal> goalsGenerated;

    public GoalGenerator(int numberOfGoalsToAdd) {
        // this.goalsIfcTags = goalsIfcTags;
        this.numberOfGoalsToAdd = numberOfGoalsToAdd;
    }

    private static void clearAll() {
        clearGoals();
        clearGoalsFromSigns();
    }

    private static void clearGoals() {
        Utility.RemoveAllComponentsOfType<AgentGoal>();
    }

    private static void clearGoalsFromSigns() {
        Utility.RemoveAllComponentsOfType<SignboardDirections>();
    }
    
    private List<AgentGoal> generateRandomGoals() {
        List<AgentGoal> goalsGenerated = new ();
        GameObject[] candidates = getGoalCandidates();
        candidates.Shuffle();
        int goalsToAdd = Mathf.Min(numberOfGoalsToAdd, candidates.Length);
        
        for (int i = 0; i < goalsToAdd; i++) {
            GameObject goalCandidate = candidates[i];
            AgentGoal goal = goalCandidate.AddComponent<AgentGoal>();
            goalsGenerated.Add(goal);
        }

        return goalsGenerated;
    }

    public void AddGoalsToSigns(float percentageOfUsefulSigns) {
        clearGoalsFromSigns();
        
        foreach (AgentGoal goal in goalsGenerated) {
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

    // private bool ifcTagMatchesGoalCandidate(string ifcTag) {
    //     return goalsIfcTags.Contains(ifcTag);
    // }
    
    private GameObject[] getGoalCandidates() {
        List<IntermediateMarker> candidates = Object.FindObjectsOfType<IntermediateMarker>().ToList();
        return candidates.Select(candidate => candidate.gameObject).ToArray();
        
        // IFCData[] ifcObjects = Object.FindObjectsOfType<IFCData>();
        // List<IFCData> candidates =  ifcObjects.ToList();
        // candidates.RemoveAll(obj => !ifcTagMatchesGoalCandidate(obj.IFCClass));
        // return candidates.Select(candidate => candidate.gameObject).ToArray();
    }

    public AgentGoal[] GenerateGoals() {
        clearAll();
        goalsGenerated = generateRandomGoals();
        return goalsGenerated.ToArray();
    }
}
