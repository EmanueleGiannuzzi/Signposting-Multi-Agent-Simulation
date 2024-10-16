using UnityEngine;
using System.Linq;
using Unity.AI.Navigation;


[RequireComponent(typeof(VisibilityHandler))]
public class VisibilityPlaneGenerator : MonoBehaviour {
    public GameObject ifcGameObject;
    public string[] flooringIfcTags = { "IfcSlab" };

    private GameObject visibilityPlanesGroup;
    private int analysisResolution => GetComponent<VisibilityHandler>().resolution;


    private bool ShouldAnalyzeArea(string ifcClass) {
        return this.flooringIfcTags.Contains(ifcClass);
    }

    public void GenerateVisibilityPlanes() {
        DestroyImmediate(GameObject.Find(Constants.VISIBILITY_PLANES_GROUP_TAG));
        visibilityPlanesGroup = new GameObject(Constants.VISIBILITY_PLANES_GROUP_TAG) {
            tag = Constants.VISIBILITY_PLANES_GROUP_TAG
        };

        GeneratePlaneForGameObject(ifcGameObject);
    }

    private void GeneratePlaneForGameObject(GameObject goElement) {
        if(!goElement.activeSelf) {
            return;
        }

        IFCData ifcData = goElement.GetComponent<IFCData>();
        if(ifcData != null) {
            string ifClass = ifcData.IFCClass;
            if(ShouldAnalyzeArea(ifClass)) {
                GameObject plane = new GameObject(ifcData.STEPName) {
                    layer = Constants.VISIBILITY_OBJECT_LAYER // To be ignored from NavMesh
                };
                NavMeshModifier navmeshModifier = plane.AddComponent<NavMeshModifier>();
                navmeshModifier.ignoreFromBuild = true;
                MeshFilter meshFilter = plane.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = plane.AddComponent<MeshRenderer>();

                meshRenderer.material = new Material(Shader.Find("Unlit/Transparent"));

                Mesh topMesh = Utility.GetTopMeshFromGameObject(goElement, out float floorHeight);
                floorHeight += 0.0001f;

                Vector3 position = goElement.transform.position;
                position[1] = floorHeight; // the Y value
                plane.transform.position = position;

                meshFilter.mesh = topMesh;

                plane.transform.parent = visibilityPlanesGroup.transform;

                VisibilityPlaneData planeData = plane.AddComponent<VisibilityPlaneData>();
                planeData.OriginalFloorHeight = floorHeight;

                Bounds meshRendererBounds = plane.GetComponent<MeshRenderer>().bounds;
                float planeWidth = meshRendererBounds.extents.x * 2;
                float planeHeight = meshRendererBounds.extents.z * 2;

                int widthResolution = (int)Mathf.Floor(planeWidth * analysisResolution);
                int heightResolution = (int)Mathf.Floor(planeHeight * analysisResolution);

                planeData.SetResolution(widthResolution, heightResolution);
                planeData.GenerateAnalyzablePoints();
            }
        }
        foreach(Transform childTransform in goElement.transform) {
            GameObject child = childTransform.gameObject;
            GeneratePlaneForGameObject(child);
        }
    }
}
