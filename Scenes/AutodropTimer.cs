using Godot;
using System;

public class AutodropTimer : Timer
{
    [Export(PropertyHint.None)]
    float FirstLevelTimeout = 5f;
    [Export(PropertyHint.None)]
    float LevelDecay = 0.9f;
    [Export(PropertyHint.None)]
    int resetsPerLevel = 10;
    [Export(PropertyHint.None)]
    float betweenTime = 0.5f;

    bool betweenTimer = false;

    int resets = 0;
    float timeoutTime;

    public override void _Ready()
    {
        timeoutTime = FirstLevelTimeout;
    }

    public void BetweenWordsTimer(bool progressLevel)
    {
        WaitTime = betweenTime;
        Start();
        betweenTimer = true;

        if (progressLevel)
        {
            resets++;
            if (resets >= resetsPerLevel)
            {
                resets = 0;
                timeoutTime *= LevelDecay;
            }
        }

        return;
    }

    public void WordTimer()
    {
        betweenTimer = false;        
        WaitTime = timeoutTime;
        Start();

    }

    public bool isBetween
    {
        get
        {
            return betweenTimer;
        }
    }

    public float Progress {
        get { 
            if (betweenTimer) return 0;

            return 1 - TimeLeft / timeoutTime;         
        }
    }

    public float LevelProgress
    {
        get
        {
            return resets / (float)resetsPerLevel;
        }
    }

    public override string ToString()
    {
        string mode = isBetween ? "BTW " : "";
        return $"{mode}{LevelProgress:F1}/{Progress:F1}/{WaitTime:F1}";
    }
}
