using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalGenerator {
    private readonly string[] goalsIfcTags;
    private readonly int numberOfGoalsToAdd;
    private readonly float percentageOfUsefulSigns;

    public GoalGenerator(string[] goalsIfcTags, int numberOfGoalsToAdd, float percentageOfUsefulSigns) {
        this.goalsIfcTags = goalsIfcTags;
        this.numberOfGoalsToAdd = numberOfGoalsToAdd;
        this.percentageOfUsefulSigns = percentageOfUsefulSigns;
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

    private void addGoalsToSigns(List<AgentGoal> goals) {
        foreach (AgentGoal goal in goals) {
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

    private bool ifcTagMatchesGoalCandidate(string ifcTag) {
        return goalsIfcTags.Contains(ifcTag);
    }
    
    private GameObject[] getGoalCandidates() {
        IFCData[] ifcObjects = Object.FindObjectsOfType<IFCData>();
        List<IFCData> candidates =  ifcObjects.ToList();
        candidates.RemoveAll(obj => !ifcTagMatchesGoalCandidate(obj.IFCClass));
        return candidates.Select(candidate => candidate.gameObject).ToArray();
    }

    public AgentGoal[] GenerateGoals() {
        clearAll();
        List<AgentGoal> goalsGenerated = generateRandomGoals();
        addGoalsToSigns(goalsGenerated);
        return goalsGenerated.ToArray();
    }
}
