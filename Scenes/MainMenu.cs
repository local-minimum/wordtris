using Godot;
using LanguageTools;

public class MainMenu : Control
{
    Button PlayButton;
    Button HowToButton;    

    public string resource = "res://wordlist.txt";

    Thread loadThread;

    private void loadLexicon()
    {
        if (!Lexicon.Initizialized)
        {
            var resourceData = GodotHelpers.LoadTextResource.Load(resource);
            Lexicon.Init(resourceData, 7);
        }

        GetNode<Label>("LoadingLabel").Visible = false;
    }

    public override void _Ready()
    {
        PlayButton = GetNode<Button>("PlayButton");
        PlayButton.Disabled = true;
        HowToButton = GetNode<Button>("HowToButton");
        HowToButton.Disabled = true;        

        loadThread = new Thread();
        loadThread.Start(this, "loadLexicon");
    }

    public override void _Process(float delta)
    {
        if (PlayButton.Disabled && Lexicon.Initizialized)
        {
            PlayButton.Disabled = false;
            HowToButton.Disabled = false;
        }
    }

    protected override void Dispose(bool disposing)
    {
        loadThread?.WaitToFinish();

        base.Dispose(disposing);
    }
}
