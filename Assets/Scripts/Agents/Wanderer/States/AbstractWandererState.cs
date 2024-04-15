using UnityEngine;

public abstract class AbstractWandererState {
    public bool isDone { get; private set; }

    protected float startTime;
    public float runningTime => Time.time - startTime;

    public void Setup() {
        //TODO 
    }
    
    public void Initialize() {
        isDone = false;
    }

    public virtual void Enter() {
        startTime = Time.time;
    }
    public virtual void Do() {}
    public virtual void FixedDo() {}
    public virtual void Exit() {}
}