using Godot;
using System;

public class DropProgress : ColorRect
{
    public float progressHeight = 5f;
    public int ScreenWidth = 600;
    
    AutodropTimer timer;
    bool active = false;
    
    ColorRect levelProgress;

    public void Init(AutodropTimer timer)
    {
        this.timer = timer;
        active = true;
    }

    public override void _Ready()
    {
        levelProgress = GetParent().GetNode<ColorRect>("LevelProgress");
    }

    public void GameOver()
    {
        timer = null;
        active = false;
    }

    public override void _Process(float delta)
    {
        if (timer == null || !active)
        {
            RectSize = new Vector2(0, 0);
            levelProgress.RectSize = new Vector2(0, 0);
        } else
        {
            RectSize = new Vector2(timer.Progress * ScreenWidth, progressHeight);
            levelProgress.RectSize = new Vector2(timer.LevelProgress * ScreenWidth, progressHeight);
        }       
    }

    public override string ToString()
    {
        if (timer == null) return "NOT INITED";

        var acivity = active ? "ACT " : "DONE ";
        return $"{acivity}{timer.Progress:F2} {timer.LevelProgress:F2}";
    }
}
