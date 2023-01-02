using Godot;
using System;

public class AutodropTimer : Timer
{
    [Export(PropertyHint.None)]
    public float FirstLevelTimeout = 5f;
    [Export(PropertyHint.None)]
    public float LevelDecay = 0.9f;
    [Export(PropertyHint.None)]
    public int resetsPerLevel = 10;

    int resets = 0;
    float timeoutTime;

    void ResetTimeout()
    {
        WaitTime = timeoutTime;
        Start();
    }

    public void ResetTimer()
    {
        if (timeoutTime == 0)
        {
            timeoutTime = FirstLevelTimeout;
            ResetTimeout();
            return;
        }

        resets++;
        if (resets >= resetsPerLevel)
        {
            resets = 0;
            timeoutTime *= LevelDecay;
        }

        ResetTimeout();
    }
}
