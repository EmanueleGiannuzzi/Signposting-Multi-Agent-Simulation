using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IfcOpenShellParser))]
public class IfcOpenShellParserEditor : Editor
{
    private IfcOpenShellParser handler;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if(GUILayout.Button("Load IFC")) {
            handler.LoadIFC();
        }

        if(GUILayout.Button("Load OBJ, MTL, XML")) {
            handler.LoadOBJ_MTL_XMLFile();
        }

        if(GUILayout.Button("Load XML")) {
            string xmlPath = EditorUtility.OpenFilePanel("Import XML", "", "xml");

            if(!string.IsNullOrEmpty(xmlPath)) {
                handler.LoadXML(xmlPath);
            }
        }
    }

    void OnEnable() {
        handler = (IfcOpenShellParser)target;
    }
}
