using Godot;
using System;

public class SwapSceneButton : Button
{
    [Export(PropertyHint.File)]
    private string sceneName;

    public override void _Ready()
    {
        Connect("pressed", this, "OnClose");
    }

    private void OnClose()
    {        
        var result = GetTree().ChangeScene(sceneName);
        if (result != Error.Ok)
        {
            GD.PrintErr($"Failed to load {sceneName} ({result})");
        }
    }
}
