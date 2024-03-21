
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(SignboardGridGenerator))]
public class SignboardGridGeneratorEditor : GenericEditor<SignboardGridGenerator> {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if(GUILayout.Button("Generate Signboards Grid")) {
            handler.GenerateGrid();
        }
    }
    
}