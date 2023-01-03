using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using LanguageTools;

public class PlayField : CanvasLayer
{
    [Export(PropertyHint.None)]
	public bool debugWords = true;

	public string resource = "wordlist.tres";

	private Lexicon lexicon;
	private VirtualBoard virtualBoard;

	private PlayerWord PlayerWord;
	private BoardGrid BoardGrid;
	private Label Message;
	private Label WordList;
	private AutodropTimer Timer;
	private DropProgress DropProgress;
	private Label NextWord;

	int minWordLength = 3;
	int score = 0;
	private bool playing = true;
	private bool eligableForBonus = false;

	public override void _Ready()
	{
		GD.Randomize();

		BoardGrid = GetNode<BoardGrid>("BoardGrid");
		PlayerWord = GetNode<PlayerWord>("PlayerWord");
		Message = GetNode<Label>("Message");
		WordList = GetNode<Label>("WordList");
		Timer = GetNode<AutodropTimer>("AutodropTimer");
		DropProgress = GetNode<DropProgress>("DropProgress");
		NextWord = GetNode<Label>("NextWord");

		lexicon = new Lexicon(resource, BoardGrid.gridSize);

		virtualBoard = new VirtualBoard(BoardGrid.gridSize);

		Timer.Connect("timeout", this, "OnAutodrop");
		Timer.BetweenWordsTimer(false);

		DropProgress.Init(Timer);

		//Message.Text = lexicon.Size.ToString();
	}


	private void OnAutodrop()
    {
		if (!playing) return;

		if (Timer.isBetween)
        {
			NewPlayerWord();
			Timer.WordTimer();
			return;			
        }

		try
		{
			var progress = Drop();
			Timer.BetweenWordsTimer(progress);
		}
		catch (LetterCollisionException)
		{
			PlayerWord.Word = "";
			WordList.Text = "GAME OVER!";
			playing = false;
			DropProgress.GameOver();
		}
	}

	private string nextWord;

	void SetNextWord()
	{
		nextWord = lexicon.RandomGram;
		NextWord.Text = $"NEXT: {nextWord}";
	}

	private void NewPlayerWord()
    {
		if (String.IsNullOrEmpty(nextWord))
        {
			SetNextWord();			
        }
		PlayerWord.Word = nextWord;
		PlayerWord.Anchor = BoardGrid.center;
		SetNextWord();
	}

	public int ScoreWord(int wordLength) => Mathf.RoundToInt(Mathf.Pow(2, wordLength + Timer.Level - 2));

	int ScoreWords(List<Coordinates2D> range)
    {
		if (range.Count == 0) return 0;

		var horizontal = range.All(coord => coord.Y == range.First().Y);

		List<Extent2D> wordCoords = new List<Extent2D>();
		List<string> candidates = new List<string>();
		List<string> words = new List<string>();

		var wordCandidates = virtualBoard.GetWordCandidates(range, horizontal);
		while (wordCandidates.MoveNext())
        {
			var wordCandidate = wordCandidates.Current;
			var candidate = wordCandidate.Key.candidate;			
			var coords = wordCandidate.Key.coords;
			var anchor = wordCandidate.Key.anchor;
			var isRange = wordCandidate.Value;

			if (debugWords) candidates.Add(candidate);

			var word = isRange ?
				lexicon.LongestWordAroundRange(candidate, anchor, anchor + range.Count() - 1, minWordLength) :
				lexicon.LongestWordAround(candidate, anchor, minWordLength);

			if (String.IsNullOrEmpty(word)) continue;

			if (isRange)
            {
				if (horizontal)
				{
					var wordStart = new Coordinates2D(candidate.IndexOf(word) + coords.X - anchor, coords.Y);
					var wordEnd = wordStart + Coordinates2D.Right * word.Length;
					wordCoords.Add(new Extent2D(wordStart, wordEnd));
				}
				else
				{
					var wordStart = new Coordinates2D(coords.X, candidate.IndexOf(word) + coords.Y - anchor);
					var wordEnd = wordStart + Coordinates2D.Down * word.Length;
					wordCoords.Add(new Extent2D(wordStart, wordEnd));
				}
			}
			else
            {
				if (horizontal)
				{
					var wordStart = new Coordinates2D(coords.X, candidate.IndexOf(word) + coords.Y - anchor);
					var wordEnd = wordStart + Coordinates2D.Down * word.Length;
					wordCoords.Add(new Extent2D(wordStart, wordEnd));
				}
				else
				{
					var wordStart = new Coordinates2D(candidate.IndexOf(word) + coords.X - anchor, coords.Y);
					var wordEnd = wordStart + Coordinates2D.Right * word.Length;
					wordCoords.Add(new Extent2D(wordStart, wordEnd));
				}
			}

			words.Add(word);
		}

		// Clear formed words and score
		int roundScore = 0;		
		for (int i = 0, l = wordCoords.Count(); i < l; i++)
        {
			var wordExtent = wordCoords[i];

			roundScore += ScoreWord(wordExtent.Length);
			virtualBoard.Clear(wordExtent);
        }

		// Bonus
		int bonus = 0;
		if (eligableForBonus && virtualBoard.Coverage == 0)
        {
			bonus = 100;
			roundScore += bonus;
			eligableForBonus = false;
        } else if (virtualBoard.Coverage > 0) {
			eligableForBonus = true;
		}

		// Multipier
		int multiplier = words.Count();
		roundScore *= multiplier;

		// Sum up
		score += roundScore;

		// Display stuff
		Message.Text = $"Score: {score}";
		UpdateWordList(words, roundScore, candidates, bonus, multiplier);

		return roundScore;
    }

	private void UpdateWordList(List<string> words, int roundScore, List<string> candidates, int bonus, int multiplier)
    {		
		var scoredWords = words.OrderBy(w => -w.Length).Select(w => $"{ScoreWord(w.Length)}:\u00A0{w}");
		WordList.Text = $"{String.Join(", ", scoredWords)}";
		if (bonus > 0)
		{
			WordList.Text += $"\nBonus: +{bonus}";
		}
		if (roundScore > 0 && multiplier > 1)
		{
			WordList.Text += $"\nMultiplier: x{multiplier}!";
		}
		if (debugWords)
		{
			WordList.Text += $"\nCHKS: {String.Join(" ", candidates.Select(candidate => String.Join("", candidate, lexicon.isWord(candidate) ? "*" : "")))}";
		}
	}

	private bool Drop()
    {
		PlayerWord.Normalize(BoardGrid.gridSize);
		var range = virtualBoard.PlaceLettes(PlayerWord.Letters());
		var roundScore = ScoreWords(range);
		PlayerWord.Word = "";
		return roundScore > 0;
	}

	public override void _Input(InputEvent @event)
    {
		if (!playing || Timer.isBetween) return;

        if (@event.IsActionPressed("Drop"))
        {
			try
            {
				var progress = Drop();				
				Timer.BetweenWordsTimer(progress);
			}
			catch (LetterCollisionException)
            {
				return;
            }
        }
    }

    public override void _Process(float delta)
    {
		//Message.Text = Timer.ToString();

		if (PlayerWord.Dirty)
        {
			PlayerWord.Normalize(BoardGrid.gridSize);
        }

		BoardGrid.ClearField();
		BoardGrid.DrawField(virtualBoard.Letters);
		BoardGrid.DrawHover(PlayerWord.Letters());
    }
}
