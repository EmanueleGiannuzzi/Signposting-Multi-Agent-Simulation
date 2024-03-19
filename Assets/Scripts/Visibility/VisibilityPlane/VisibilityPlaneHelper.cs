
using UnityEngine;

public static class VisibilityPlaneHelper {
    public static VisibilityPlaneData[] GetVisibilityPlanes() {
        GameObject visPlaneGroup = Utility.GetGroup(Constants.VISIBILITY_GROUP_NAME);
        if (visPlaneGroup == null) {
            Debug.LogError("Unable to find Visibility Planes");
            return null;
        }
        return visPlaneGroup.GetComponentsInChildren<VisibilityPlaneData>();
    }
}