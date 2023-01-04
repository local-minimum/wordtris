using Godot;
using System;

public class PauseScreen : Control
{
    [Export(PropertyHint.None)]
    private ulong retryMillies = 1000;

    [Export(PropertyHint.None)]
    private string restartText = "Restart";

    [Export(PropertyHint.None)]
    private string pauseText = "Paused";

    private ulong retryStart;
    private Label label;

    private ColorRect retryProgress;
    private Vector2 retryProgresSize;

    public void RetryInitiated()
    {
        retryStart = Time.GetTicksMsec();
        label.Text = restartText;
    }

    bool ShouldRestart
    {
        get
        {
            return Time.GetTicksMsec() - retryStart > retryMillies;
        }
    }
    public void RetryEnded() {
        if (ShouldRestart)
        {
            Restart();            
        } else
        {
            retryStart = 0;
            label.Text = pauseText;
            retryProgress.RectSize = Vector2.Zero;
        }        
    }

    void Restart()
    {
        var result = GetParent().GetTree().ReloadCurrentScene();
        if (result != Error.Ok)
        {
            GD.Print("Failed to reload scene");
        }
        return;
    }

    public override void _Ready()
    {
        label = GetNode<Label>("Label");
        label.Text = pauseText;

        retryProgress = GetNode<ColorRect>("RestartProgress");
        retryProgresSize = retryProgress.RectSize;
        retryProgress.RectSize = Vector2.Zero;
    }

    private void SizeProgressBar()
    {
        float progress = (float)(Time.GetTicksMsec() - retryStart) / retryMillies;
        retryProgress.RectSize = new Vector2(progress * retryProgresSize.x, retryProgresSize.y);
    }

    public override void _Process(float delta)
    {
        if (retryStart != 0 && ShouldRestart) Restart();
        else if (retryStart != 0) SizeProgressBar(); 
    }
}
