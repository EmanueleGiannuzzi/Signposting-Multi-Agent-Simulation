using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VisibilityHandler : MonoBehaviour {
    private VisibilityPlaneGenerator VisibilityPlaneGenerator;
    
    [SerializeField] private bool generateVisibilityDataOnStart = false;
    
    [Header("Agent Types(Eye level)")]
    public StringFloatTuple[] agentTypes;
    
    [Header("Resolution (point/meter)")]
    public int resolution = 10;

    private Dictionary<Vector2Int, VisibilityInfo>[][] visibilityInfo;//1 for each visibility plane mesh
    
    [Header("Texture Color")]
    public Color nonVisibleColor = new(1f, 1f, 1f, 0.5f);
    private Texture2D[,] resultTextures; //[visPlaneId, agentTypeID]
    
    [HideInInspector]
    public float progressAnalysis = -1f;

    public bool IsCoverageReady { get; private set; } = false;

    private void OnValidate() {
        IsCoverageReady = false;
    }

    private void Awake() {
        VisibilityPlaneGenerator = FindObjectOfType<VisibilityPlaneGenerator>();
    }

    private void Start() {
        if (generateVisibilityDataOnStart) {
            VisibilityPlaneGenerator.GenerateVisibilityPlanes();
            GenerateVisibilityData();
        }
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
        int visPlaneSize = getVisibilityPlanes().Length;
        visibilityInfo = new Dictionary<Vector2Int, VisibilityInfo>[visPlaneSize][];
        
        for(int i = 0; i < visPlaneSize; i++) {
            visibilityInfo[i] = new Dictionary<Vector2Int, VisibilityInfo>[agentTypes.Length];
            for(int j = 0; j < agentTypes.Length; j++) {
                visibilityInfo[i][j] = new Dictionary<Vector2Int, VisibilityInfo>();
            }
        }
        
        int signboardsFound = getSignboardArray().Length;
        if(signboardsFound <= 0) {
            Debug.LogWarning("No Signboards found.");
            return;
        }
        Debug.Log($"{signboardsFound} Signboards found.");

        analyzeSignboards();
        generateTextures();
        
        IsCoverageReady = true;
        Debug.Log("Done Calculating Visibility Areas");
        
        ShowVisibilityPlane(0);
    }
    
    private Texture2D textureFromVisibilityData(Dictionary<Vector2Int, VisibilityInfo> visibilityData, int width, int height) {
        Texture2D texture = new Texture2D(width, height);

        foreach((Vector2Int coords, VisibilityInfo visInfo) in visibilityData) {
            Color visibleColor = new Color(0, 0, 0, 0);
            foreach(IFCSignBoard signboard in visInfo.GetVisibleBoards()) {
                visibleColor += signboard.Color;
            }
            visibleColor /= visInfo.GetVisibleBoards().Count;
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

                Transform visPlaneTransform = visibilityPlane.transform;
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
    
    public void CalculateSignCoverage(VisibilityPlaneData[] visibilityPlanes) {
        foreach (IFCSignBoard signboard in SignboardHelper.GetIFCSignboards()) {
            SignboardVisibility signboardVisibility = signboard.GetComponent<SignboardVisibility>();
            signboardVisibility.CoveragePerAgentType = new float[agentTypes.Length];
        }

        for (int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
            int visPlaneSize = visibilityInfo.Length;
            for(int visPlaneId = 0; visPlaneId < visPlaneSize; visPlaneId++) {
                Dictionary<Vector2Int, VisibilityInfo>[] visInfo = visibilityInfo[visPlaneId];
                Dictionary<Vector2Int, VisibilityInfo> visInfoDictionary = visInfo[agentTypeID];
                foreach(VisibilityInfo singleVisInfo in visInfoDictionary.Values) {
                    foreach (IFCSignBoard signboard in singleVisInfo.GetVisibleBoards()) {
                        SignboardVisibility signboardVisibility = signboard.GetComponent<SignboardVisibility>();
                        signboardVisibility.CoveragePerAgentType[agentTypeID]++;
                    }
                }
            }
        }
    }
    
    public List<IFCSignBoard> GetSignboardsVisible(Vector3 worldPosition, int agentTypeID) {
        if(visibilityInfo == null) {
            Debug.LogError("Visibility Info Unavailable");
            return new List<IFCSignBoard>();
        }
        
        float positionSensitivity = 1f / resolution;
        
        VisibilityPlaneData[] visibilityPlanes = getVisibilityPlanes();
        int visPlaneSize = visibilityPlanes.Length;
        for(int visPlaneId = 0; visPlaneId < visPlaneSize; visPlaneId++) {
            Dictionary<Vector2Int, VisibilityInfo>[] visInfo = visibilityInfo[visPlaneId];
            Dictionary<Vector2Int, VisibilityInfo> visInfoDictionary = visInfo[agentTypeID];
            foreach(VisibilityInfo value in visInfoDictionary.Values) {
                float headingX = value.CachedWorldPos.x - worldPosition.x;
                float headingZ = value.CachedWorldPos.z - worldPosition.z;

                float distanceSquared = headingX * headingX + headingZ * headingZ;
                if(distanceSquared < positionSensitivity * positionSensitivity) {
                    return value.GetVisibleBoards();
                }
            }
        }
        return new List<IFCSignBoard>();
    }
    
    public static bool IsSignboardInFOV(Transform agentTransform, IFCSignBoard signboard, float agentFOVDeg) {
        Vector2 signboardPos = Utility.Vector3ToVector2NoY(signboard.transform.position);
        Vector2 agentPos = Utility.Vector3ToVector2NoY(agentTransform.position);

        Vector2 directionLooking = Utility.Vector3ToVector2NoY(agentTransform.forward);
        Vector2 directionSignboard = (signboardPos - agentPos).normalized;

        float angleToPoint = Mathf.Rad2Deg * Mathf.Acos(Vector2.Dot(directionLooking, directionSignboard));

        return angleToPoint <= agentFOVDeg / 2;
    }

    public void ShowVisibilityPlane(int agentTypeID) {
        Texture2D[] textures = GetTexturesFromAgentID(agentTypeID);
        float agentEyeLevel = agentTypes[agentTypeID].Value;
        VisibilityPlaneHelper.ApplyTextureVisibilityPlane(agentEyeLevel, textures);
    }
}