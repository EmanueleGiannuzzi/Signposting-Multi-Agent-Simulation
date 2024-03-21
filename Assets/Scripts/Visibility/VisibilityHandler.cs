using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VisibilityHandler : MonoBehaviour {
    [Header("Agent Types(Eye level)")]
    public StringFloatTuple[] agentTypes;
    
    [Header("Resolution (point/meter)")]
    public int resolution = 10;
    
    public Dictionary<Vector2Int, VisibilityInfo>[][] visibilityInfo;//1 for each visibility plane mesh
    
    [Header("Texture Color")]
    public Color nonVisibleColor = new(1f, 0f, 0f, 0.5f);//red
    private Texture2D[,] resultTextures; //[visPlaneId, agentTypeID]
    
    [HideInInspector]
    public float progressAnalysis = -1f;

    public bool IsCoverageReady { get; private set; } = false;

    private void OnValidate() {
        IsCoverageReady = false;
    }

    public void ClearAllData() {
        visibilityInfo = null;
        resultTextures = null;
        
        IsCoverageReady = false;
    }

    private static VisibilityPlaneData[] getVisibilityPlanes() {
        return VisibilityPlaneHelper.GetVisibilityPlanes();
    }

    private static SignboardVisibility[] getSignboardArray() {
        return SignboardHelper.GetSignboardVisibilities();
    }
    
    private static IFCSignBoard[] getIFCSignboardArray() {
        return SignboardHelper.GetIFCSignboards();
    }

    public void GenerateVisibilityData() {
        int signboardsFound = getSignboardArray().Length;
        if(signboardsFound <= 0) {
            Debug.LogError("No Signboards found.");
            return;
        }
        Debug.Log($"{signboardsFound} Signboards found.");
        
        int visPlaneSize = getVisibilityPlanes().Length;
        visibilityInfo = new Dictionary<Vector2Int, VisibilityInfo>[visPlaneSize][];
        for(int i = 0; i < visPlaneSize; i++) {
            visibilityInfo[i] = new Dictionary<Vector2Int, VisibilityInfo>[agentTypes.Length];
            for(int j = 0; j < agentTypes.Length; j++) {
                visibilityInfo[i][j] = new Dictionary<Vector2Int, VisibilityInfo>();
            }
        }

        analyzeSignboards();
        generateTextures();
        
        IsCoverageReady = true;
        Debug.Log("Done Calculating Visibility Areas");
    }
    
    private Texture2D textureFromVisibilityData(Dictionary<Vector2Int, VisibilityInfo> visibilityData, int width, int height) {
        Texture2D texture = new Texture2D(width, height);

        foreach((Vector2Int coords, VisibilityInfo visibilityInfo) in visibilityData) {
            Color visibleColor = new Color(0, 0, 0, 0);
            foreach(IFCSignBoard signboard in visibilityInfo.GetVisibleBoards()) {
                visibleColor += signboard.Color;
            }
            visibleColor /= visibilityInfo.GetVisibleBoards().Count;
            visibleColor.a = nonVisibleColor.a;

            texture.SetPixel(coords.x, coords.y, visibleColor);
        }
        texture.Apply();
        return texture;
    }

    private void generateTextures() {
        VisibilityPlaneData[] visibilityPlanes = getVisibilityPlanes();
        int visPlaneSize = visibilityPlanes.Length;
        
        resultTextures = new Texture2D[visPlaneSize, agentTypes.Length];
        
        for (int visPlaneId = 0; visPlaneId < visPlaneSize; visPlaneId++) {
            VisibilityPlaneData visibilityPlane = visibilityPlanes[visPlaneId];
            Dictionary<Vector2Int, VisibilityInfo>[] visInfos = this.visibilityInfo[visPlaneId];
            
            int widthResolution = visibilityPlane.GetAxesResolution().x;
            int heightResolution = visibilityPlane.GetAxesResolution().y;
    
            for (int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
                resultTextures[visPlaneId, agentTypeID] = textureFromVisibilityData(visInfos[agentTypeID], widthResolution, heightResolution);
            }
        }
    }
    
    public Texture2D[] GetTexturesFromAgentID(int agentTypeID) {
        return Utility.Get2DMatrixColumn(resultTextures, agentTypeID);
    }

    private void analyzeSignboards() {
        VisibilityPlaneData[] visibilityPlanes = getVisibilityPlanes();
        int visPlaneSize = visibilityPlanes.Length;
        
        this.progressAnalysis = 1f;
        float progressBarStep = 1f / visPlaneSize / agentTypes.Length / getSignboardArray().Length;
        
        for(int visPlaneId = 0; visPlaneId < visPlaneSize; visPlaneId++) {
            Dictionary<Vector2Int, VisibilityInfo>[] visInfos = this.visibilityInfo[visPlaneId];
    
            VisibilityPlaneData visibilityPlane = visibilityPlanes[visPlaneId];
            float originalFloorHeight = visibilityPlane.OriginalFloorHeight;
    
            IFCSignBoard[] ifcSignboardsArray = getIFCSignboardArray();
            for(int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
                StringFloatTuple tuple = agentTypes[agentTypeID];

                var visPlaneTransform = visibilityPlane.transform;
                Vector3 position = visPlaneTransform.position;
                position[1] = originalFloorHeight + tuple.Value; // the Y value
                visPlaneTransform.position = position;
                
                
                foreach(IFCSignBoard signboard in ifcSignboardsArray) {
                    Vector3 p = signboard.WorldCenterPoint;
                    Vector3 n = signboard.Direction;
                    float theta = (signboard.ViewingAngle * Mathf.PI) / 180;
                    float d = signboard.ViewingDistance;
    
                    foreach(Vector2 vi2 in visibilityPlane.GetPointsForAnalysis().Keys) {
                        Vector3 vi = new Vector3(vi2.x, visibilityPlane.transform.position.y, vi2.y);
    
                        bool isVisible = false;
    
                        Vector3 pToViDirection = vi - p;
    
                        if((Vector3.Dot((vi - p), n) / ((vi - p).magnitude * n.magnitude)) >= Mathf.Cos(theta / 2) && ((vi - p).magnitude <= d)) {
                            Ray ray = new Ray(p, pToViDirection);
                            float maxDistance = Vector3.Distance(p, vi);
                            if(!Physics.Raycast(ray, out _, maxDistance)) {
                                isVisible = true;
                            }
                        }
    
                        if(isVisible) {
                            Vector2Int coordsToSave = visibilityPlane.GetPointsForAnalysis()[vi2];
                            if(visInfos[agentTypeID].ContainsKey(coordsToSave)) {
                                visInfos[agentTypeID][coordsToSave].AddVisibleBoard(signboard);
                            }
                            else {
                                VisibilityInfo visInfo = new VisibilityInfo(vi);
                                visInfo.AddVisibleBoard(signboard);
                                visInfos[agentTypeID].Add(coordsToSave, visInfo);
                            }
                        }
                    }
    
                    this.progressAnalysis -= progressBarStep;
                    if(EditorUtility.DisplayCancelableProgressBar("Visibility Handler", "Generating Signboard Visibility Data", 1f - progressAnalysis)) {
                        EditorUtility.ClearProgressBar();
                        this.progressAnalysis = -1f;
                        return;
                    }
                }
    
            }
        }
    
        CalculateSignCoverage(visibilityPlanes);
        EditorUtility.ClearProgressBar();
    
        this.progressAnalysis = -1f;
    }
    
    
    public void CalculateSignCoverage(VisibilityPlaneData[] visibilityPlanes) {//TODO: Rewrite from scratch
        // int visPlaneSize = visibilityPlanes.Length;
        //
        // int[,] signboardCoverage = new int[getSignboardArray().Length, agentTypes.Length];
        // int visibilityGroupMaxSize = 0;
        // for(int visPlaneId = 0; visPlaneId < visPlaneSize; visPlaneId++) {
        //     Dictionary<Vector2Int, VisibilityInfo>[] visInfosPerMesh = this.visibilityInfo[visPlaneId];
        //     for(int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
        //         Dictionary<Vector2Int, VisibilityInfo> visInfosPerAgentType = visInfosPerMesh[agentTypeID];
        //         foreach(KeyValuePair<Vector2Int, VisibilityInfo> entry in visInfosPerAgentType) {
        //             signboardCoverage[signboardID, agentTypeID] += entry.Value.GetVisibleBoards().Count;
        //         }
        //     }
        //     visibilityGroupMaxSize += visibilityPlanes[visPlaneId].ValidMeshPointsCount;
        // }
        //
        // SignboardVisibility[] signboardsArray = getSignboardArray();
        // int signboardsArraySize = signboardsArray.Length;
        //
        // for(int signboardID = 0; signboardID < signboardsArraySize; signboardID++) {
        //     SignboardVisibility signboard = signboardsArray[signboardID];
        //     signboard.CoveragePerAgentType = new float[agentTypes.Length];
        //     for(int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
        //         float coverage = (float)signboardCoverage[signboardID, agentTypeID] / visibilityGroupMaxSize;
        //         signboard.CoveragePerAgentType[agentTypeID] = coverage;
        //     }
        // }
    }
    
    public List<IFCSignBoard> GetSignboardIDsVisible(Vector3 agentPosition, int agentTypeID) {
        if(visibilityInfo == null) {
            Debug.LogError("Visibility Info Unavailable");
            return null;
        }
        
        float positionSensitivity = 1f / resolution;
        
        VisibilityPlaneData[] visibilityPlanes = getVisibilityPlanes();
        int visPlaneSize = visibilityPlanes.Length;
        for(int visPlaneId = 0; visPlaneId < visPlaneSize; visPlaneId++) {
            Dictionary<Vector2Int, VisibilityInfo>[] visInfo = visibilityInfo[visPlaneId];
            Dictionary<Vector2Int, VisibilityInfo> visInfoDictionary = visInfo[agentTypeID];
            foreach(VisibilityInfo value in visInfoDictionary.Values) {
                float headingX = value.CachedWorldPos.x - agentPosition.x;
                float headingZ = value.CachedWorldPos.z - agentPosition.z;

                float distanceSquared = headingX * headingX + headingZ * headingZ;
                if(distanceSquared < positionSensitivity * positionSensitivity) {
                    return value.GetVisibleBoards();
                }
            }
        }
        return new List<IFCSignBoard>();
    }
}