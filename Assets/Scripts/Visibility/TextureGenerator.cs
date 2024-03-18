
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TextureGenerator {
    private Texture2D[][] resultTextures;

    private Gradient gradient;
    
    private void generateTextures() {
        resultTextures = new Texture2D[GetVisibilityPlaneSize(), agentTypes.Length];
        
        for (int visPlaneId = 0; visPlaneId < GetVisibilityPlaneSize(); visPlaneId++) {
            GameObject visibilityPlane = GetVisibilityPlane(visPlaneId);
            Dictionary<Vector2Int, VisibilityInfo>[] visInfos = this.visibilityInfos[visPlaneId];
            VisibilityPlaneData planeData = visibilityPlane.GetComponent<VisibilityPlaneData>();
            
            int widthResolution = planeData.GetAxesResolution().x;
            int heightResolution = planeData.GetAxesResolution().y;
    
            for (int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
                resultTextures[visPlaneId][agentTypeID] = TextureGenerator.TextureFromVisibilityData(visInfos[agentTypeID],
                    GetSignboardArray(), widthResolution, heightResolution, nonVisibleColor);
            }
        }
    }
    
    public Texture2D GetResultTexture(int visPlaneId, int agentTypeID) {
        return resultTextures[visPlaneId][agentTypeID];
    }
    
    private static Texture2D textureFromColourMap(Color[] colourMap, int width, int height) {
        Texture2D texture = new Texture2D(width, height) {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }
    
    public static Texture2D BestSignboardTexture(GameObject signboardGridGroup, int agentTypeID, int visPlaneID, int width, int height, float minVisibility, float maxVisibility, Gradient gradient) {
        Color[] colorMap = new Color[width * height];
        Utility.FillArray(colorMap, gradient.Evaluate(0f));

        foreach(Transform child in signboardGridGroup.transform.GetChild(visPlaneID)) {
            SignBoard signboard = child.gameObject.GetComponent<SignBoard>();
            GridSignageboard gridSignboard = child.gameObject.GetComponent<GridSignageboard>();

            float visibility = signboard.GetVisiblityForHeatmap()[agentTypeID];
            float visiblityNorm = (visibility / (maxVisibility - minVisibility)) + minVisibility;

            colorMap[gridSignboard.planeLocalIndex.y * width + gridSignboard.planeLocalIndex.x] = gradient.Evaluate(visiblityNorm);
        }

        return textureFromColourMap(colorMap, width, height);
    }

    private static Texture2D textureFromVisibilityDataNormalized(float[,] dataMatrix, Gradient gradient) {
        float[,] dataMatrixNormalized = Utility.NormalizeData01(dataMatrix);
        
        
    }
}