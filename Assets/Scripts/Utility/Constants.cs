
public static class Constants {
    public const int NAVMESH_NOT_WALKABLE_AREA_TYPE = 1;
    
    
    //Layers
    public const int AGENTS_LAYER = 7;
    public const int VISIBILITY_OBJECT_LAYER = 8;
    public const int WALKABLE_LAYER = 9;
    public const int MARKERS_LAYER = 10;
    
    //Layer Masks
    public const int ONLY_MARKERS_LAYER_MASK = 1 << MARKERS_LAYER;
    public const int ONLY_AGENTS_LAYER_MASK = 1 << AGENTS_LAYER;
    public const int ALL_BUT_AGENTS_LAYER_MASK = ~ONLY_AGENTS_LAYER_MASK;
    
    //Tags
    public const string MARKERS_TAG = "Markers";
    
    //Group Names
    public const string MARKERS_GROUP_NAME = "MarkersGroup";
    public const string VISIBILITY_GROUP_NAME = "VisibilityPlanesGroup";
}