using Godot;
using GodotHelpers;
using LanguageTools;

public class MainMenu : Control
{
    Button PlayButton;

    public string resource = "res://wordlist.txt";

    
    private void loadLexicon()
    {
        if (Lexicon.Unloaded)
        {
            SimpleTimer.Start("Lexicon Load");
            GD.Print($"Loading lexicon from: {resource}");
            var resourceFile = LoadTextResource.Load(resource);
            Lexicon.Init(resourceFile, 7);
        }
    }

    public override void _Ready()
    {
        PlayButton = GetNode<Button>("PlayButton");
        PlayButton.Disabled = true;

        loadLexicon();
    }

    public override void _Process(float delta)
    {
        if (Lexicon.Inizializing)
        {
            Lexicon.LoadBatch();
            GetNode<Label>("LoadingLabel").Text = Lexicon.InitStatus;
            return;
        }

        if (PlayButton.Disabled && Lexicon.Loaded)
        {
            PlayButton.Disabled = false;

            GetNode<Label>("LoadingLabel").Visible = false;
            GD.Print("Lexicon loaded");
            SimpleTimer.Stop("Lexicon Load");
        }
    }
}
