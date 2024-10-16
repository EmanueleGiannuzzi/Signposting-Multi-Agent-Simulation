using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WandererValidation))]
public class WandererValidationEditor : GenericEditor<WandererValidation> {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        // if (GUILayout.Button("Generate Goals")) {
        //     handler.InitGoalGenerator();
        // }

        if (Application.isPlaying && GUILayout.Button("Start Tests")) {
            handler.StartTests();
        }
    }
}
