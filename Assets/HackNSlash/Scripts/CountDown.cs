using UnityEngine;

class CountDown
{
    private float finishTime = 0;

    public float TimeToFinish() { return finishTime - Time.fixedTime; }

    public bool HasFinished() { return finishTime <= Time.fixedTime; }

    public void Stop()
    {
        finishTime = 0;
    }

    public void Restart(float timeAmount)
    {
        finishTime = Time.fixedTime + timeAmount + Time.fixedDeltaTime;
    }
}
