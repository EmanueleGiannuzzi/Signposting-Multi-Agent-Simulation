using UnityEngine;

public class SignboardVisibility : MonoBehaviour {
    [Header("Analysis Result")]
    [SerializeField] private float[] _coveragePerAgentType;//[0,1]
    [SerializeField] private float[] _visibilityPerAgentType;//[0,1]

    public float[] CoveragePerAgentType {
        get => _coveragePerAgentType;
        set => _coveragePerAgentType = value;
    }
    public float[] VisibilityPerAgentType {
        get => _visibilityPerAgentType;
        set => _visibilityPerAgentType = value;
    }
}