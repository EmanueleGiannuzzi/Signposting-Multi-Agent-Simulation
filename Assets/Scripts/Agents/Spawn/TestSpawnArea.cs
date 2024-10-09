using UnityEngine;
using UnityEngine.UIElements;

public class TestSpawnArea : SpawnArea {
    private Camera PlayerCamera;
    public new GameObject AgentPrefab;


    public MouseButton mouseButton;

    private new void Start() {
        base.Start();
        PlayerCamera = FindObjectOfType<Camera>();
    }

    void Update() {
        if(Input.GetMouseButtonDown((int)mouseButton)) {
            Ray ray = PlayerCamera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit)) {
                SpawnAgentMoveTo(AgentPrefab, hit.point, null);
            }
        }
    }

    protected override bool ShouldSpawnAgents() {
        return false;
    }
}
