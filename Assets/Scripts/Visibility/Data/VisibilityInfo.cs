using System.Collections.Generic;
using UnityEngine;

public class VisibilityInfo {
    private readonly Vector3 cachedWorldPos;
    private readonly List<int> visibleBoards = new();

    public VisibilityInfo(Vector3 worldPos) {
        this.cachedWorldPos = worldPos;
    }

    public void AddVisibleBoard(int boardID) {
        visibleBoards.Add(boardID);
    }

    public List<int> GetVisibleBoards() {
        return visibleBoards;
    }
}
