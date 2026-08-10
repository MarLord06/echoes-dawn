using UnityEngine;

public class PlaytimeTracker : MonoBehaviour
{
    public float playtimeInSeconds = 0f;
    private bool isTracking = true;

    void Update()
    {
        if (isTracking)
            playtimeInSeconds += Time.deltaTime;
    }

    public void SetPlaytime(float time)
    {
        playtimeInSeconds = time;
    }

    public void StopTracking()
    {
        isTracking = false;
    }

    public void StartTracking()
    {
        isTracking = true;
    }
}
