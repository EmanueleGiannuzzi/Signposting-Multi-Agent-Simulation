
using UnityEngine;

public static class VisibilityPlaneHelper {
    public static VisibilityPlaneData[] GetVisibilityPlanes() {
        GameObject visPlaneGroup = Utility.GetGroup(Constants.VISIBILITY_PLANES_GROUP_TAG);
        if (visPlaneGroup == null) {
            Debug.LogError("Unable to find Visibility Planes");
            return null;
        }
        return visPlaneGroup.GetComponentsInChildren<VisibilityPlaneData>();
    }
    
    public static void ApplyTextureVisibilityPlane(float agentEyeLevel, Texture2D[] textures) {
        VisibilityPlaneData[] visibilityPlanes = GetVisibilityPlanes();

        for(int visPlaneId = 0; visPlaneId < visibilityPlanes.Length; visPlaneId++) {
            VisibilityPlaneData visibilityPlane = visibilityPlanes[visPlaneId];

            var visibilityPlaneTransform = visibilityPlane.transform;
            Vector3 position = visibilityPlaneTransform.position;
            float originalFloorHeight = visibilityPlane.OriginalFloorHeight;
            position[1] = originalFloorHeight + agentEyeLevel;
            visibilityPlaneTransform.position = position;

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
            meshRenderer.sharedMaterial.mainTexture = textures[visPlaneId];
        }
    }
}