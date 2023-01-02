using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LanguageTools;

public class PlayField : CanvasLayer
{
	public string resource = "wordlist.tres";

	private HashSet<string> lexicon = new HashSet<string>();

	private GramBag bigrams;
	private GramBag trigrams;
	private RandomNumberGenerator RNG;

	public int TriToBiRatio = 3;

	private PlayerWord PlayerWord;
	private BoardGrid BoardGrid;
	private Label Message;
	private Label WordList;

	private string[,] LetterGrid;

	int score = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Randomize();
		RNG = new RandomNumberGenerator();
		RNG.Randomize();

		List<string> wordList = _LoadResource();
		lexicon.UnionWith(wordList);

		Dictionary<string, int> bigramsDict = new Dictionary<string, int>();
		Dictionary<string, int> trigramsDict = new Dictionary<string, int>();
		wordList.CreateGrams(bigramsDict);
		wordList.CreateGrams(trigramsDict, 3);
		bigrams = new GramBag(bigramsDict);
		trigrams = new GramBag(trigramsDict);

		BoardGrid = GetNode<BoardGrid>("BoardGrid");
		PlayerWord = GetNode<PlayerWord>("PlayerWord");
		Message = GetNode<Label>("Message");
		WordList = GetNode<Label>("WordList");

		LetterGrid = new string[BoardGrid.gridSize, BoardGrid.gridSize];

		NewPlayerWord();		
	}

	private void NewPlayerWord()
    {
		PlayerWord.Word = RandomGram;
		PlayerWord.Anchor = BoardGrid.center;
	}

	public string RandomGram
	{
		get
		{
			if (RNG.RandiRange(0, TriToBiRatio) == 0)
			{
				return bigrams.WeightedRandomGram;
			}
			return trigrams.WeightedRandomGram;
		}
	}


	private string LongestWordAround(string input, int newCharacterPosition, int minLength = 2)
	{
		return input
			.SubstringsAround(newCharacterPosition, minLength)
			.OrderBy(w => -w.Length)
			.Where(w => lexicon.Contains(w))
			.FirstOrDefault();
	}

	private string LongestWordAroundRange(string input, int firstNewCharacterPosition, int lastNewCharacterPosition, int minLength)
	{
		return input
			.SubstringsAroundRange(firstNewCharacterPosition, lastNewCharacterPosition, minLength)
			.OrderBy(w => -w.Length)
			.Where(w => lexicon.Contains(w))
			.FirstOrDefault();
	}

	private List<string> _LoadResource()
	{
		string rawContents;
		using (StreamReader streamReader = new StreamReader(resource, Encoding.UTF8))
		{
			rawContents = streamReader.ReadToEnd();
		}
		return rawContents
			.Split(new string[] { "\n" }, StringSplitOptions.None)
			.Select(x => x.Trim())
			.Where(x => x.Length > 0)
			.ToList();
	}

	List<Coordinates2D> PlaceLettes(IEnumerator<Letter> letters)
    {		
		var letterList = new List<Letter>();

		while (letters.MoveNext())
        {
			var letter = letters.Current;
			if (String.IsNullOrEmpty(LetterGrid[letter.Coordinates.X, letter.Coordinates.Y]))
			{
				LetterGrid[letter.Coordinates.X, letter.Coordinates.Y] = letter.Character;
				letterList.Add(letter);
			}
			else {
				for (int i = 0, l=letterList.Count; i<l; i++)
                {
					letter = letterList[i];
					LetterGrid[letter.Coordinates.X, letter.Coordinates.Y] = "";
				}
				throw new LetterCollisionException();
			}
		}

		return letterList.Select(letter => letter.Coordinates).ToList();
    }

	IEnumerator<Letter> Letters
    {
		get
        {
			for (var x = 0; x < LetterGrid.GetLength(0); x++)
            {
				for (var y = 0; y < LetterGrid.GetLength(1); y++)
                {
					var character = LetterGrid[x, y];
					if (!String.IsNullOrEmpty(character))
                    {
						yield return new Letter(new Coordinates2D(x, y), character);
                    }
                }
            }
        }
    }
	struct WordCandidate
    {
		public string candidate;
		public int anchor;

		public WordCandidate(string candidate, int anchor)
        {
			this.candidate = candidate;
			this.anchor = anchor;
        }

        public override string ToString()
        {
			return $"{candidate} {anchor}";
        }
    }

	WordCandidate VerticalCandidate(Coordinates2D coordinates)
    {
		var size = BoardGrid.gridSize;
		var x = coordinates.X;
		var y = coordinates.Y;
		var minY = y;
		var maxY = y;

		while (minY > 0 && !String.IsNullOrEmpty(LetterGrid[x, minY - 1]))
        {
			minY--;
        }
		while (maxY < size - 1 && !String.IsNullOrEmpty(LetterGrid[x, maxY + 1])) {
			maxY++;
        }

		var candidate = "";
		for (var i = minY; i<=maxY; i++)
        {
			candidate += LetterGrid[x, i];
        }
		return new WordCandidate(candidate, y - minY);
    }

	WordCandidate HorizontalCandidate(Coordinates2D coordinates)
    {
		var size = BoardGrid.gridSize;
		var x = coordinates.X;
		var y = coordinates.Y;
		var minX = x;
		var maxX = x;

		while (minX > 0 && !String.IsNullOrEmpty(LetterGrid[minX - 1, y]))
		{
			minX--;
		}
		while (maxX < size - 1 && !String.IsNullOrEmpty(LetterGrid[maxX + 1, y]))
		{
			maxX++;
		}

		var candidate = "";
		for (var i = minX; i <= maxX; i++)
		{
			candidate += LetterGrid[i, y];
		}
		return new WordCandidate(candidate, x - minX);

	}

	public int ScoreWord(int wordLength) => Mathf.RoundToInt(Mathf.Pow(2, wordLength));

	void ScoreWords(List<Coordinates2D> range)
    {
		int minLength = 3;
		List<Extent2D> wordCoords = new List<Extent2D>();
		List<string> words = new List<string>();

		// Orthogonals
		var horizontal = range.All(coord => coord.Y == range.First().Y);
		for (int i = 0, l = range.Count(); i < l; i++)
        {
			var coords = range[i];
			var candidate = horizontal ? VerticalCandidate(coords) : HorizontalCandidate(coords);

			if (String.IsNullOrEmpty(candidate.candidate)) continue;

			var word = LongestWordAround(candidate.candidate, candidate.anchor, minLength);
			if (String.IsNullOrEmpty(word)) continue;

			if (horizontal)
			{
				var wordStart = new Coordinates2D(coords.X, candidate.candidate.IndexOf(word) + coords.Y - candidate.anchor);
				var wordEnd = wordStart + Coordinates2D.Down * word.Length;
				wordCoords.Add(new Extent2D(wordStart, wordEnd));
			} else
            {
				var wordStart = new Coordinates2D(candidate.candidate.IndexOf(word) + coords.X - candidate.anchor, coords.Y);
				var wordEnd = wordStart + Coordinates2D.Right * word.Length;
				wordCoords.Add(new Extent2D(wordStart, wordEnd));
			}

			words.Add(word);
		}

		// Parallells
		var rangeSeed = range[0];
		var rangeCandidate = horizontal ? HorizontalCandidate(rangeSeed) : VerticalCandidate(rangeSeed);

		if (!String.IsNullOrEmpty(rangeCandidate.candidate))
        {
			var rangeWord = LongestWordAroundRange(rangeCandidate.candidate, rangeCandidate.anchor, rangeCandidate.anchor + range.Count(), minLength);

			if (!String.IsNullOrEmpty(rangeWord))
            {				
				if (horizontal)
				{
					var wordStart = new Coordinates2D(rangeCandidate.candidate.IndexOf(rangeWord) + rangeSeed.X - rangeCandidate.anchor, rangeSeed.Y);
					var wordEnd = wordStart + Coordinates2D.Right * rangeWord.Length;
					wordCoords.Add(new Extent2D(wordStart, wordEnd));
				}
				else
				{
					var wordStart = new Coordinates2D(rangeSeed.X, rangeCandidate.candidate.IndexOf(rangeWord) + rangeSeed.Y - rangeCandidate.anchor);
					var wordEnd = wordStart + Coordinates2D.Down * rangeWord.Length;
					wordCoords.Add(new Extent2D(wordStart, wordEnd));
				}

				words.Add(rangeWord);
			}
		}
		

		// Clear it formed words
		var anchor = Coordinates2D.Zero;
		for (int i = 0, l = wordCoords.Count(); i < l; i++)
        {
			var word = wordCoords[i];

			score += ScoreWord(word.Length);

			for (int j = 0, k = word.Length; j<k; j++)
            {
				var coord = word.Interpolate(anchor, j);
				LetterGrid[coord.X, coord.Y] = "";
            }
        }

		Message.Text = $"Score: {score}";
		var scoredWords = words.OrderBy(w => -w.Length).Select(w => $"{ScoreWord(w.Length)}: {w}");
		WordList.Text = $"{String.Join(", ", scoredWords)}";
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Drop"))
        {
			try
            {
				var range = PlaceLettes(PlayerWord.Letters());
				ScoreWords(range);
				NewPlayerWord();
			}
			catch (LetterCollisionException)
            {
				return;
            }
        }
    }

    public override void _Process(float delta)
    {
		if (PlayerWord.Dirty)
        {
			PlayerWord.Normalize(BoardGrid.gridSize);
        }

		BoardGrid.ClearField();
		BoardGrid.DrawField(Letters);
		BoardGrid.DrawField(PlayerWord.Letters());
    }
}
