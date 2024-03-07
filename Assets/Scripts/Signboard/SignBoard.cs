using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Transform))]
//[ExecuteInEditMode]
//[InitializeOnLoad]
public class SignBoard : MonoBehaviour {
    [Header("Signage Board Parameters")]
    public float ViewingDistance;
    public float ViewingAngle;
    public Color Color = Color.clear;
    public float MinimumReadingTime;

    [Header("Analysys Result")]
    public float[] coveragePerAgentType;//[0,1]
    public float[] visibilityPerAgentType;//[0,1]

    [Header("Debug")]
    public int views = 0;
    public int agentsSpawned = 0;

    public void Start() {
        if(Color == Color.clear) {
            Color = new Color(
             Random.Range(0f, 1f),
             Random.Range(0f, 1f),
             Random.Range(0f, 1f)
           );
        }
    }

    public float[] GetVisiblityForHeatmap() {
        return visibilityPerAgentType;
    }


    public Vector3 GetDirection() {
        return this.transform.up;
    }

    public Vector3 GetWorldCenterPoint() {
        return this.transform.position;
    }

    public float GetViewingAngle() {
        return ViewingAngle;
    }

    public float GetViewingDistance() {
        return ViewingDistance;
    }

    public Color GetColor() {
        return Color;
    }

    public void CopyDataFrom(SignBoard source) {
        this.ViewingDistance = source.ViewingDistance;
        this.ViewingAngle = source.ViewingAngle;
        this.Color = source.Color;
        this.MinimumReadingTime = source.MinimumReadingTime;
    }
}
