
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VisibilityHandler))]
public class VisibilityHandlerEditor : GenericEditor<VisibilityHandler> {
    private int showPlaneSliderValue = 0;
    
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        
        if(GUILayout.Button("Calculate Sign Coverage")) {
            handler.GenerateVisibilityData();
        }
        
        if(handler.agentTypes is not { Length: > 0 } || !handler.IsCoverageReady) {
            GUI.enabled = false;
            GUILayout.Button("Show Plane: 0");
            GUI.enabled = true;
        }
        else {
            EditorGUILayout.BeginHorizontal("box");
            StringFloatTuple agentType = handler.agentTypes[showPlaneSliderValue];
            float agentEyeLevel = agentType.Value;
            if(GUILayout.Button($"Show Planes for Agent Type: {agentType.Key} ({agentEyeLevel})")) {
                Texture2D[] textures = handler.GetTexturesFromAgentID(showPlaneSliderValue);
                VisibilityPlaneHelper.ApplyTextureVisibilityPlane(agentEyeLevel, textures);
            }
            if(handler.agentTypes.Length > 1) {
                showPlaneSliderValue = (int)Mathf.Round(GUILayout.HorizontalSlider(showPlaneSliderValue, 0, handler.agentTypes.Length - 1));
            }
            EditorGUILayout.EndHorizontal();
        }
        
        if(GUILayout.Button("Clear Data")) {
            handler.ClearAllData();
        }
    }
}