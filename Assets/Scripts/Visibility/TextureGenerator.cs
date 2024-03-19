
using System;
using UnityEngine;

[Serializable]
public class TextureGenerator {
    private Texture2D[][] resultTextures;

    private Gradient gradient;

    // private void generateTextures() {
    //     resultTextures = new Texture2D[GetVisibilityPlaneSize(), agentTypes.Length];
    //     
    //     for (int visPlaneId = 0; visPlaneId < GetVisibilityPlaneSize(); visPlaneId++) {
    //         GameObject visibilityPlane = GetVisibilityPlane(visPlaneId);
    //         Dictionary<Vector2Int, VisibilityInfo>[] visInfos = this.visibilityInfos[visPlaneId];
    //         VisibilityPlaneData planeData = visibilityPlane.GetComponent<VisibilityPlaneData>();
    //         
    //         int widthResolution = planeData.GetAxesResolution().x;
    //         int heightResolution = planeData.GetAxesResolution().y;
    //
    //         for (int agentTypeID = 0; agentTypeID < agentTypes.Length; agentTypeID++) {
    //             resultTextures[visPlaneId][agentTypeID] = TextureGenerator.TextureFromVisibilityData(visInfos[agentTypeID],
    //                 GetSignboardArray(), widthResolution, heightResolution, nonVisibleColor);
    //         }
    //     }
    // }
    
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
    
    // public static Texture2D BestSignboardTexture(GameObject signboardGridGroup, int agentTypeID, int visPlaneID, int width, int height, float minVisibility, float maxVisibility, Gradient gradient) {
    //     Color[] colorMap = new Color[width * height];
    //     Utility.FillArray(colorMap, gradient.Evaluate(0f));
    //
    //     foreach(Transform child in signboardGridGroup.transform.GetChild(visPlaneID)) {
    //         SignBoard signboard = child.gameObject.GetComponent<SignBoard>();
    //         GridSignboard gridSignboard = child.gameObject.GetComponent<GridSignboard>();
    //
    //         float visibility = signboard.GetVisiblityForHeatmap()[agentTypeID];
    //         float visiblityNorm = (visibility / (maxVisibility - minVisibility)) + minVisibility;
    //
    //         colorMap[gridSignboard.planeLocalIndex.y * width + gridSignboard.planeLocalIndex.x] = gradient.Evaluate(visiblityNorm);
    //     }
    //
    //     return textureFromColourMap(colorMap, width, height);
    // }

    private static Texture2D textureFromDataMatrix2D(float[,] dataMatrix, Gradient gradient) {
        float[,] dataMatrixNormalized = Utility.NormalizeData01(dataMatrix);
        int rows = dataMatrixNormalized.GetLength(0);
        int cols = dataMatrixNormalized.GetLength(1);

        Texture2D texture = new Texture2D(rows, cols);
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                texture.SetPixel(i,j, gradient.Evaluate(dataMatrixNormalized[i,j]));
            }
        }
        return texture;
    }
}