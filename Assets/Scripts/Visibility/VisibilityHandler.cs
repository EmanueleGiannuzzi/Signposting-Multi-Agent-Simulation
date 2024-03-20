using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VisibilityHandler : MonoBehaviour {
    [Header("Agent Types(Eye level)")]
    public StringFloatTuple[] agentTypes;
    
    [Header("Resolution (point/meter)")]
    public int resolution = 10;
    
    public Dictionary<Vector2Int, VisibilityInfo>[][] visibilityInfos;//1 for each visibility plane mesh
    
    [Header("Texture Color")]
    public Color nonVisibleColor = new(1f, 0f, 0f, 0.5f);//red
    private Texture2D[,] resultTextures;
    
    [HideInInspector]
    public float progressAnalysis = -1f;
    private bool _done = false;
    public bool IsCoverageReady => _done;

    public void ClearAllData() {
        visibilityInfos = null;
        resultTextures = null;
        
        _done = false;
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
        if(getSignboardArray().Length <= 0) {
            Debug.LogError("No Signboards found, please press Init first.");
            return;
        }
        
        int visPlaneSize = getVisibilityPlanes().Length;
        visibilityInfos = new Dictionary<Vector2Int, VisibilityInfo>[visPlaneSize][];
        for(int i = 0; i < visPlaneSize; i++) {
            visibilityInfos[i] = new Dictionary<Vector2Int, VisibilityInfo>[agentTypes.Length];
            for(int j = 0; j < agentTypes.Length; j++) {
                visibilityInfos[i][j] = new Dictionary<Vector2Int, VisibilityInfo>();
            }
        }

        analyzeSignboards();
        generateTextures();
        
        _done = true;
        Debug.Log("Done Calculating Visibility Areas");
    }
    
    private Texture2D textureFromVisibilityData(Dictionary<Vector2Int, VisibilityInfo> visibilityData, int width, int height) {
        Texture2D texture = new Texture2D(width, height);

        IFCSignBoard[] signboards = getIFCSignboardArray();
        foreach((Vector2Int coords, VisibilityInfo visibilityInfo) in visibilityData) {
            Color visibleColor = new Color(0, 0, 0, 0);
            foreach(int signboardID in visibilityInfo.GetVisibleBoards()) {
                visibleColor += signboards[signboardID].Color;
            }
            visibleColor /= visibilityInfo.GetVisibleBoards().Count;

            visibleColor.a = nonVisibleColor.a;

            texture.SetPixel(coords.x, coords.y, visibleColor);
        }
        return texture;
    }

    private void generateTextures() {
        VisibilityPlaneData[] visibilityPlanes = getVisibilityPlanes();
        int visPlaneSize = visibilityPlanes.Length;
        
        resultTextures = new Texture2D[visPlaneSize, agentTypes.Length];
        
        for (int visPlaneId = 0; visPlaneId < visPlaneSize; visPlaneId++) {
            VisibilityPlaneData visibilityPlane = visibilityPlanes[visPlaneId];
            Dictionary<Vector2Int, VisibilityInfo>[] visInfos = this.visibilityInfos[visPlaneId];
            
            int widthResolution = visibilityPlane.GetAxesResolution().x;
            int heightResolution = visibilityPlane.GetAxesResolution().y;
    
            for (int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
                resultTextures[visPlaneId, agentTypeID] = textureFromVisibilityData(visInfos[agentTypeID], widthResolution, heightResolution);
            }
        }
    }
    
    private Texture2D getResultTexture(int visPlaneId, int agentTypeID) {
        return resultTextures[visPlaneId, agentTypeID];
    }
    
    public void ShowVisibilityPlane(int agentTypeID) {
        if(this.visibilityInfos == null) {
            return;
        }
        
        VisibilityPlaneData[] visibilityPlanes = getVisibilityPlanes();
        int visPlaneSize = visibilityPlanes.Length;
        for(int visPlaneId = 0; visPlaneId < visPlaneSize; visPlaneId++) {
            VisibilityPlaneData visibilityPlane = visibilityPlanes[visPlaneId];

            var visPlaneTransform = visibilityPlane.transform;
            Vector3 position = visPlaneTransform.position;
            float originalFloorHeight = visibilityPlane.OriginalFloorHeight;
            position[1] = originalFloorHeight + agentTypes[agentTypeID].Value; // the Y value
            visPlaneTransform.position = position;
    
            Bounds meshRendererBounds = visibilityPlane.GetComponent<MeshRenderer>().bounds;
            
            Vector3[] meshVertices = visibilityPlane.GetComponent<MeshFilter>().sharedMesh.vertices;
            Vector2[] uvs = new Vector2[meshVertices.Length];
    
            Vector3 localMin = visibilityPlane.transform.InverseTransformPoint(meshRendererBounds.min);
            Vector3 localMax = visibilityPlane.transform.InverseTransformPoint(meshRendererBounds.max) - localMin;
    
            for(int i = 0; i < meshVertices.Length; i++) {
                Vector3 normVertex = meshVertices[i] - localMin;
                uvs[i] = new Vector2(1f - normVertex.x / localMax.x, 1f - normVertex.z / localMax.z);
            }
            visibilityPlane.GetComponent<MeshFilter>().sharedMesh.uv = uvs;
    
            MeshRenderer meshRenderer = visibilityPlane.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial.mainTexture = getResultTexture(visPlaneId, agentTypeID);
        }
    }

    private void analyzeSignboards() {
        VisibilityPlaneData[] visibilityPlanes = getVisibilityPlanes();
        int visPlaneSize = visibilityPlanes.Length;
        
        this.progressAnalysis = 1f;
        float progressBarStep = 1f / visPlaneSize / agentTypes.Length / getSignboardArray().Length;
        
        for(int visPlaneId = 0; visPlaneId < visPlaneSize; visPlaneId++) {
            Dictionary<Vector2Int, VisibilityInfo>[] visInfos = this.visibilityInfos[visPlaneId];
    
            VisibilityPlaneData visibilityPlane = visibilityPlanes[visPlaneId];
            float originalFloorHeight = visibilityPlane.OriginalFloorHeight;
    
            IFCSignBoard[] ifcSignboardsArray = getIFCSignboardArray();
            int ifcSignboardsArraySize = ifcSignboardsArray.Length;
            for(int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
                StringFloatTuple tuple = agentTypes[agentTypeID];

                var visPlaneTransform = visibilityPlane.transform;
                Vector3 position = visPlaneTransform.position;
                position[1] = originalFloorHeight + tuple.Value; // the Y value
                visPlaneTransform.position = position;
                
                
                for(int signboardID = 0; signboardID < ifcSignboardsArraySize; signboardID++) {
                    IFCSignBoard signBoard = ifcSignboardsArray[signboardID];
                    Vector3 p = signBoard.WorldCenterPoint;
                    Vector3 n = signBoard.Direction;
                    float theta = (signBoard.ViewingAngle * Mathf.PI) / 180;
                    float d = signBoard.ViewingDistance;
    
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
                                visInfos[agentTypeID][coordsToSave].AddVisibleBoard(signboardID);
                            }
                            else {
                                VisibilityInfo vinfo = new VisibilityInfo(vi);
                                vinfo.AddVisibleBoard(signboardID);
                                visInfos[agentTypeID].Add(coordsToSave, vinfo);
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
    
    
    public void CalculateSignCoverage(VisibilityPlaneData[] visibilityPlanes) {
        int visPlaneSize = visibilityPlanes.Length;
        
        int[,] signboardCoverage = new int[getSignboardArray().Length, agentTypes.Length];
        int visibilityGroupMaxSize = 0;
        for(int visPlaneId = 0; visPlaneId < visPlaneSize; visPlaneId++) {
            Dictionary<Vector2Int, VisibilityInfo>[] visInfosPerMesh = this.visibilityInfos[visPlaneId];
            for(int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
                Dictionary<Vector2Int, VisibilityInfo> visInfosPerAgentType = visInfosPerMesh[agentTypeID];
                foreach(KeyValuePair<Vector2Int, VisibilityInfo> entry in visInfosPerAgentType) {
                    foreach(int signboardID in entry.Value.GetVisibleBoards()) {
                        signboardCoverage[signboardID, agentTypeID]++;
                    }
                }
            }
            visibilityGroupMaxSize += visibilityPlanes[visPlaneId].ValidMeshPointsCount;
        }

        SignboardVisibility[] signboardsArray = getSignboardArray();
        int signboardsArraySize = signboardsArray.Length;
        
        for(int signboardID = 0; signboardID < signboardsArraySize; signboardID++) {
            SignboardVisibility signboard = signboardsArray[signboardID];
            signboard.CoveragePerAgentType = new float[agentTypes.Length];
            for(int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
                float coverage = (float)signboardCoverage[signboardID, agentTypeID] / visibilityGroupMaxSize;
                signboard.CoveragePerAgentType[agentTypeID] = coverage;
            }
        }
    }
}