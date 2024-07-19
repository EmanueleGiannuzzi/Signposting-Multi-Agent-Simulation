using UnityEditor;
using UnityEngine;

public class EditorCameraBillboard : MonoBehaviour {
    void Update() {
        transform.LookAt(SceneView.lastActiveSceneView.camera.transform);
    }
}
