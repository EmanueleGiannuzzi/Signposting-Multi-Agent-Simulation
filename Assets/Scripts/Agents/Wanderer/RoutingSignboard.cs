using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(IFCSignBoard))]
public class RoutingSignboard : MonoBehaviour {
    [SerializeField] private Vector3 pointedDirection = Vector3.up;

    private void OnDrawGizmos() {//TODO: Test
        const float arrowLength = 1f;
        Handles.color = Color.red;
        Handles.ArrowHandleCap(0, this.transform.position, Quaternion.LookRotation(pointedDirection), arrowLength, EventType.Repaint);
    }
}