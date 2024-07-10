using UnityEngine;

public interface IRouteMarker {
    public Vector3 Position { get; }
    public string Name => getName();
    

    protected string getName();
}