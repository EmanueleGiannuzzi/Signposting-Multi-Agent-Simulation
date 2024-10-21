using System.Collections.Generic;
using UnityEngine;

public class IFCData : MonoBehaviour {
    //TODO: Use this instead of FindObjectsOfType<IFCData>, but it doesn't work in Editor
    public static HashSet<IFCData> DataCache { get; } = new(); 
    
    public new Collider collider;
    
    public string IFCClass;
    public string STEPName;
    public string STEPId;
    public string STEPIndex;
    public string IFCLayer;

    public List<IFCPropertySet> propertySets;
    public List<IFCPropertySet> quantitySets;

    private void Awake() {
        DataCache.Add(this);
        collider = GetComponent<Collider>();
    }

    private void OnDestroy() {
        DataCache.Remove(this);
    }
}