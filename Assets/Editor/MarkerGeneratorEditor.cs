using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MarkerGenerator))]
public class MarkerGeneratorEditor : GenericEditor<MarkerGenerator> {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Markers")) {
            handler.GenerateMarkers();
        }
    }
}
