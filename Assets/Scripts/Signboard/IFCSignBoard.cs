using UnityEngine;

public class IFCSignBoard : MonoBehaviour {
    [Header("Signage Board Parameters")]
    public float ViewingDistance;
    public float ViewingAngle;
    public Color Color = Color.clear;
    public float MinimumReadingTime;

    public void Start() {
        if(Color == Color.clear) {
            Color = new Color(
             Random.Range(0f, 1f),
             Random.Range(0f, 1f),
             Random.Range(0f, 1f)
           );
        }
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

    public void CopyDataFrom(IFCSignBoard source) {
        this.ViewingDistance = source.ViewingDistance;
        this.ViewingAngle = source.ViewingAngle;
        this.Color = source.Color;
        this.MinimumReadingTime = source.MinimumReadingTime;
    }
}
