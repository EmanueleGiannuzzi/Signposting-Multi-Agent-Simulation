using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class GoalGenerator : MonoBehaviour {
    [SerializeField] private string[] goalsIfcTags = {"IfcFurnishingElement"};
    [SerializeField] private int numberOfGoalsToAdd = 0;
    [SerializeField] private float percentageOfUsefulSigns = 0f;
    

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
        clearAll();

        List<AgentGoal> goalsGenerated = new ();
        foreach (GameObject goalCandidate in getGoalCandidates()) {
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

    private IFCSignBoard[] getSignboards() {
        return FindObjectsOfType<IFCSignBoard>();
    }

    private bool ifcTagMatchesGoalCandidate(string ifcTag) {
        return goalsIfcTags.Contains(ifcTag);
    }
    
    private GameObject[] getGoalCandidates() {
        IFCData[] ifcObjects = FindObjectsOfType<IFCData>();
        List<IFCData> candidates =  ifcObjects.ToList();
        candidates.RemoveAll(obj => !ifcTagMatchesGoalCandidate(obj.IFCClass));
        return candidates.Select(candidate => candidate.gameObject).ToArray();
    }
}
