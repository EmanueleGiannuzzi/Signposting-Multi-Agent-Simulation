using UnityEngine;

public class TimeController : MonoBehaviour {
    public int TargetFPS = 30;

    private const int FPS_BUFFER_SIZE = 10;
    private const float UPDATE_FREQUENCY = 1f / FPS_BUFFER_SIZE;
    private float elapsed = 0f;

    private readonly CircularBuffer<float> fpsBuffer = new(FPS_BUFFER_SIZE);

    public float CurrentFPS;
    public float Multiplier;


    private void Start() {
        elapsed = 0f;
    }

    private void Update() {
        elapsed += Time.deltaTime;

        CurrentFPS = 1f / Time.deltaTime;
        fpsBuffer.PushBack(CurrentFPS);
        if (elapsed > UPDATE_FREQUENCY) {
            elapsed = 0f;

            float currentFPSAvg = CurrentFPSAverage();
            AdjustTimeScale(currentFPSAvg);
        }
    }

    private float CurrentFPSAverage() {
        float avgFPS = 0f;
        foreach (float fpsCount in fpsBuffer) {
            avgFPS += fpsCount;
        }
        avgFPS /= fpsBuffer.Size;
        return avgFPS;
    }
    
    private void AdjustTimeScale(float avgFPS) {
        float scaleFactor = avgFPS / TargetFPS;
        float newTimeScale = Time.timeScale * scaleFactor;
        Time.timeScale = Mathf.Clamp(newTimeScale, 1f, 100f);

        Multiplier = Time.timeScale;
    }
}