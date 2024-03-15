using UnityEngine;

public class IFCSignBoard : MonoBehaviour {
    [Header("Signage Board Parameters")] 
    [SerializeField] private float _viewingDistance;
    [SerializeField] private float _viewingAngle;
    [SerializeField] private Color _color;
    [SerializeField] private float _minimumReadingTime;
    
    public float ViewingDistance { 
        get => _viewingDistance;
        set => _viewingDistance = value; 
    }
    public float ViewingAngle { 
        get => _viewingAngle;
        set => _viewingAngle = value; 
    }
    public Color Color { 
        get => _color;
        set => _color = value; 
    }
    public float MinimumReadingTime { 
        get => _minimumReadingTime;
        set => _minimumReadingTime = value; 
    }

    public void Start() {
        if(Color == Color.clear) {//TODO: Do this from spawner code
            Color = new Color(
             Random.Range(0f, 1f),
             Random.Range(0f, 1f),
             Random.Range(0f, 1f)
           );
        }
    }

    public void CopyDataFrom(IFCSignBoard source) {
        this.ViewingDistance = source.ViewingDistance;
        this.ViewingAngle = source.ViewingAngle;
        this.Color = source.Color;
        this.MinimumReadingTime = source.MinimumReadingTime;
    }
}
