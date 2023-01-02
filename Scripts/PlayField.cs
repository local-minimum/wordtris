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

    public override void _Process(float delta)
    {
		BoardGrid.DrawField(PlayerWord.Letters());
    }
}
