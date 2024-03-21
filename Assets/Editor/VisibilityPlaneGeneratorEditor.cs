
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VisibilityPlaneGenerator))]
public class VisibilityPlaneGeneratorEditor : GenericEditor<VisibilityPlaneGenerator> {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        
        if(GUILayout.Button("Generate Visibility Planes")) {
            handler.GenerateVisibilityPlanes();
            if(VisibilityPlaneHelper.GetVisibilityPlanes().Length > 0) {
                Debug.Log("Visibility Planes Generated");
            }
            else {
                Debug.LogError("Unable to generate Visibility Planes");
            }
            
            
        }
    }
}
