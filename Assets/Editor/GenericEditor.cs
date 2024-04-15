
using System;
using UnityEditor;

public class GenericEditor<T> : Editor {
    protected T handler;
    
    void OnEnable() {
        handler = (T)Convert.ChangeType(target, typeof(T));
    }
}