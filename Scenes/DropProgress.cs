using Godot;
using System;

public class DropProgress : ColorRect
{
    public int ScreenWidth = 600;
    public AutodropTimer timer;
    bool active = true;

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
        } else
        {
            RectSize = new Vector2(timer.Progress * ScreenWidth, 5f);
        }       
    }
}
