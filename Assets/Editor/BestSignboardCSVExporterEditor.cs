
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BestSignboardCSVExporter))]
public class BestSignboardCSVExporterEditor : GenericEditor<BestSignboardCSVExporter> {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Export Coverage")) {
            handler.ExportCSV();
        }
    }
}