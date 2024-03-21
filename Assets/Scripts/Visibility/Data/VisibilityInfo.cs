using System.Collections.Generic;
using UnityEngine;

public class VisibilityInfo {
    private readonly List<IFCSignBoard> visibleBoards = new();
    public Vector3 CachedWorldPos { get; }

    public VisibilityInfo(Vector3 worldPos) {
        this.CachedWorldPos = worldPos;
    }

    public void AddVisibleBoard(IFCSignBoard boardID) {
        visibleBoards.Add(boardID);
    }

    public List<IFCSignBoard> GetVisibleBoards() {
        return visibleBoards;
    }
}
