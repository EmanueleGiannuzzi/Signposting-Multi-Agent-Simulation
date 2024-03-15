using System.Collections.Generic;
using UnityEngine;

public class VisibilityHandler : MonoBehaviour {
    [Header("Agent Types(Eye level)")]
    public StringFloatTuple[] agentTypes;
    
    [Header("Resolution (point/meter)")]
    public int resolution = 10;
    
    public Dictionary<Vector2Int, VisibilityInfo>[][] visibilityInfos;//1 for each visibility plane mesh
    
    
}