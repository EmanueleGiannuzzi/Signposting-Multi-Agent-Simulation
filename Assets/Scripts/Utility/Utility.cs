using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using Object = UnityEngine.Object;
using Random = System.Random;

public static class Utility {
    private static readonly Random rng = new();
    
    //TODO: Move
    public static bool PolyContainsPoint(Vector3[] polyPoints, Vector3 p) {
        int j = polyPoints.Length - 1;
        bool inside = false;
        for(int i = 0; i < polyPoints.Length; j = i++) {
            Vector3 pi = polyPoints[i];
            Vector3 pj = polyPoints[j];
            if(((pi.z <= p.z && p.z < pj.z) || (pj.z <= p.z && p.z < pi.z)) &&
                (p.x < (pj.x - pi.x) * (p.z - pi.z) / (pj.z - pi.z) + pi.x))
                inside = !inside;
        }
        return inside;
    }

    //TODO: Move
    public static bool HorizontalPlaneContainsPoint(Mesh mesh, Vector3 aLocalPoint, float squareWidthLength, float squareHeightLenght) {
        Vector3[] verts = mesh.vertices;
        int[] tris = mesh.triangles;
        int triangleCount = tris.Length / 3;
        for(int i = 0; i < triangleCount; i++) {
            Vector3[] trianglePoly = { verts[tris[i * 3]], verts[tris[i * 3 + 1]], verts[tris[i * 3 + 2]] };

            if(PolyContainsPoint(trianglePoly, aLocalPoint)
                || PolyContainsPoint(trianglePoly, new Vector3(aLocalPoint.x - squareWidthLength, aLocalPoint.y, aLocalPoint.z - squareHeightLenght))
                || PolyContainsPoint(trianglePoly, new Vector3(aLocalPoint.x - squareWidthLength, aLocalPoint.y, aLocalPoint.z + squareHeightLenght))
                || PolyContainsPoint(trianglePoly, new Vector3(aLocalPoint.x + squareWidthLength, aLocalPoint.y, aLocalPoint.z - squareHeightLenght))
                ) {
                return true;
            }
        }
        return false;
    }
    
    //TODO: Move
    public static Mesh GetTopMeshFromGameObject(GameObject gameObject, out float floorHeight) {
        MeshFilter goMeshFilter = gameObject.GetComponent<MeshFilter>();
        if(goMeshFilter == null || goMeshFilter.sharedMesh == null) {
            floorHeight = 0f;
            return null;
        }

        Mesh goMesh = goMeshFilter.sharedMesh;
        float higherCoord = -float.MaxValue;
        foreach(Vector3 vertex in goMesh.vertices) {
            if(vertex.z > higherCoord) {
                higherCoord = vertex.z;
            }
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> invalidVerticesIDs = new List<int>();
        List<int> triangles = new List<int>();
        Dictionary<int, int> conversionTable = new Dictionary<int, int>();

        int j = 0;//New array id
        for(int i = 0; i < goMesh.vertices.Length; i++) {
            Vector3 vertex = goMesh.vertices[i];
            if(vertex.z == higherCoord) {
                Vector3 v = new Vector3(-vertex.x, vertex.y, 0);
                vertices.Add(v);
                conversionTable.Add(i, j);
                j++;
            }
            else {
                invalidVerticesIDs.Add(i);
            }
        }

        int triangleCount = goMesh.triangles.Length / 3;
        for(int i = 0; i < triangleCount; i++) {
            int v1 = goMesh.triangles[i * 3];
            int v2 = goMesh.triangles[i * 3 + 1];
            int v3 = goMesh.triangles[i * 3 + 2];

            if(!(invalidVerticesIDs.Contains(v1)
                || invalidVerticesIDs.Contains(v2)
                || invalidVerticesIDs.Contains(v3))) {//If triangle is valid
                triangles.Add(conversionTable[v1]);
                triangles.Add(conversionTable[v3]);//Reverse order(Counter-clockwise)
                triangles.Add(conversionTable[v2]);
            }
        }

        Vector3[] meshVertices = vertices.ToArray();

        Quaternion newRotation = new Quaternion {
            eulerAngles = new Vector3(-90, 0, 0)
        };
        for(int i = 0; i < meshVertices.Length; i++) {
            meshVertices[i] = newRotation * meshVertices[i];
        }

        Mesh mesh = new Mesh {
            vertices = meshVertices,
            triangles = triangles.ToArray()
        };

        mesh.name = goMesh.name;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        floorHeight = higherCoord;

        return mesh;
    }
    
    public static void DestroyObject(Object obj) {
        if (obj != null) {
#if UNITY_EDITOR
            Object.DestroyImmediate(obj);
#else
            Object.Destroy(obj);
#endif
        }
    }
    
#region Math
    public static Vector2 Vector3ToVector2NoY(Vector3 v3) {
        return new Vector2(v3.x, v3.z);
    }
        
    public static float[,] NormalizeData01(float[,] dataMatrix) {
        int rows = dataMatrix.GetLength(0);
        int cols = dataMatrix.GetLength(1);

        float dataMin = float.MaxValue;
        float dataMax = float.MinValue;
        foreach (float data in dataMatrix) {
            if (data < dataMin) 
                dataMin = data;
            if (data > dataMax)
                dataMax = data;
        }
        float range = dataMax - dataMin;
        if (range == 0)
            return dataMatrix;
            
        float[,] normalizedMatrix = new float[rows, cols];
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                normalizedMatrix[i, j] = (dataMatrix[i, j] - dataMin) / range;
            }
        }
        return normalizedMatrix;
    }

    public static float[] Normalize(float[] array, bool shouldInvert = false) {
        float min = array.Min();
        float max = array.Max();
        float range = max - min;

        if (range == 0) {
            return array.Select(_ => shouldInvert ? 1f : 0f).ToArray();
        }
        
        float[] normalizedArray = array.Select(value => (value - min) / range).ToArray();
        return shouldInvert ? normalizedArray.Select(value => 1f - value).ToArray() : normalizedArray;
    }
    
    public static float[] Normalize(int[] array, bool shouldInvert = false) {
        int min = array.Min();
        int max = array.Max();
        int range = max - min;

        if (range == 0) {
            return array.Select(_ => shouldInvert ? 1f : 0f).ToArray();
        }
        
        float[] normalizedArray = array.Select(value => (value - min) / (float)range).ToArray();
        return shouldInvert ? normalizedArray.Select(value => 1f - value).ToArray() : normalizedArray;
    }
    
    public static float[] Normalize(float[] array, Func<float, float> function, bool shouldInvert = false) {
        float[] expArray = array.Select(function).ToArray();
        return Normalize(array, shouldInvert);
    }
#endregion

#region Collections
    public static void Shuffle<T>(this IList<T> list) {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            (list[k], list[n]) = (list[n], list[k]);
        }  
    }
    
    public static void FillArray<T>(this T[] arr, T value) {
        for(int i = 0; i < arr.Length; ++i) {
            arr[i] = value;
        }
    }
    
    public static T[] RemoveAt<T>(this T[] source, int index) {
        T[] dest = new T[source.Length - 1];
        if(index > 0)
            Array.Copy(source, 0, dest, 0, index);

        if(index < source.Length - 1)
            Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }
    
    public static T[] Get2DMatrixColumn<T>(T[,] matrix, int columnNumber) {
        return Enumerable.Range(0, matrix.GetLength(0))
            .Select(x => matrix[x, columnNumber])
            .ToArray();
    }

    public static T[] Get2DMatrixRow<T>(T[,] matrix, int rowNumber) {
        return Enumerable.Range(0, matrix.GetLength(1))
            .Select(x => matrix[rowNumber, x])
            .ToArray();
    }
    
    public static int GetRandomWeightedIndex(float[] weights) {
        float weightSum = 0f;
        foreach (float weight in weights) {
            weightSum += weight;
        }
 
        int index = 0;
        int lastIndex = weights.Length - 1;
        while (index < lastIndex) {
            if (UnityEngine.Random.Range(0, weightSum) < weights[index]) {
                return index;
            }
            weightSum -= weights[index++];
        }
        return index;
    }
    
    public static IEnumerable<T> Subtract<T>(this IEnumerable<T> orgList, IEnumerable<T> toRemove) {
        List<T> list = orgList.ToList();
        foreach(T x in toRemove) {
            list.Remove(x);
        }
        return list;
    }
#endregion

#region OS
    public static void DeleteFile(string path) {
        if(File.Exists(path)) {
            File.Delete(path);
        }
    }
    
    public static System.Diagnostics.Process RunCommand(string commandFileName, string arg, bool showConsole) {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

        if(!showConsole) {
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = false;
        }

        startInfo.FileName = commandFileName;
        startInfo.Arguments = arg;
        process.StartInfo = startInfo;

        try {
            process.Start();
        }
        catch(Exception ex) {
            throw new Exception("Error starting process", ex);
        }

        return process;
    }

#endregion

#region Groups
    public static GameObject CreateNewGroup(string groupTag) {
        return CreateNewGroup(groupTag, groupTag);
    }
    
    public static GameObject CreateNewGroup(string groupTag, string groupName) {
        if (GetGroup(groupTag) != null) {
            Debug.LogWarning($"An Object with tag {groupTag} have been already created. Deleting it.");
            DeleteGroup(groupTag);
        } 
        
        GameObject newGroup = new GameObject(groupName) {
            tag = groupTag
        };
        return newGroup;
    }

    [CanBeNull]
    public static GameObject GetGroup(string groupTag) {
        return GameObject.FindGameObjectWithTag(groupTag);
    }

    public static void DeleteGroup(string groupTag) {
        GameObject group = GetGroup(groupTag);
        DestroyObject(group);
    }
    
    public static T[] GetComponentsFromTag<T>(string tag) {
        GameObject[] objectsArray = GameObject.FindGameObjectsWithTag(tag);
        if (objectsArray == null) {
            return null;
        }

        int arraySize = objectsArray.Length;
        T[] componentsArray = new T[arraySize];
        for (int i = 0; i < arraySize; i++) {
            componentsArray[i] = objectsArray[i].GetComponent<T>();
            if (componentsArray[i] == null) {
                Debug.LogError($"Component of type {nameof(T)} not find in object {objectsArray[i].name}, with tag {tag}");
            }
        }
        return componentsArray;
    }
#endregion
    
}
