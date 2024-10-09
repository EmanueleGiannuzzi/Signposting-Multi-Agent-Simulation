using UnityEngine;

public interface IRouteMarker {
    public Vector3 Position => GetPosition();
    public string Name => GetName();
    
    public string GetName();
    public Vector3 GetPosition();

    public GameObject GetGameObject();
}