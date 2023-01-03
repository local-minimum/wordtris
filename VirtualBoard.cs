using System;
using System.Collections.Generic;
using System.Linq;

class VirtualBoard
{
	public struct WordCandidate
	{
		public string candidate;
		public int anchor;
		public Coordinates2D coords;

		public WordCandidate(string candidate, int anchor, Coordinates2D coords)
		{
			this.candidate = candidate;
			this.anchor = anchor;
			this.coords = coords;
		}

		public override string ToString()
		{
			return $"{candidate} {anchor}";
		}
	}

	private int size;
	private string[,] LetterGrid;

    public VirtualBoard(int size)
    {
        LetterGrid = new string[size, size];
		this.size = size;
    }

	public List<Coordinates2D> PlaceLettes(IEnumerator<Letter> letters)
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
			else
			{
				for (int i = 0, l = letterList.Count; i < l; i++)
				{
					letter = letterList[i];
					LetterGrid[letter.Coordinates.X, letter.Coordinates.Y] = "";
				}
				throw new LetterCollisionException();
			}
		}

		return letterList.Select(letter => letter.Coordinates).ToList();
	}

	public IEnumerator<Letter> Letters
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
	public WordCandidate VerticalCandidate(Coordinates2D coordinates)
	{		
		var x = coordinates.X;
		var y = coordinates.Y;
		var minY = y;
		var maxY = y;

		while (minY > 0 && !String.IsNullOrEmpty(LetterGrid[x, minY - 1]))
		{
			minY--;
		}
		while (maxY < size - 1 && !String.IsNullOrEmpty(LetterGrid[x, maxY + 1]))
		{
			maxY++;
		}

		var candidate = "";
		for (var i = minY; i <= maxY; i++)
		{
			candidate += LetterGrid[x, i];
		}
		return new WordCandidate(candidate, y - minY, coordinates);
	}

	public WordCandidate HorizontalCandidate(Coordinates2D coordinates)
	{
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
		return new WordCandidate(candidate, x - minX, coordinates);
	}

	private Coordinates2D origo = Coordinates2D.Zero;

	public void Clear(Extent2D wordExtent)
    {
		for (int j = 0, k = wordExtent.Length; j < k; j++)
		{
			var coord = wordExtent.Interpolate(origo, j);			
			LetterGrid[coord.X, coord.Y] = "";
		}
	}

	public IEnumerator<KeyValuePair<WordCandidate, bool>> GetWordCandidates(List<Coordinates2D> range, bool horizontal)
    {
		for (int i = 0, l = range.Count(); i < l; i++)
		{
			var coords = range[i];
			var candidate = horizontal ? VerticalCandidate(coords) : HorizontalCandidate(coords);

			if (String.IsNullOrEmpty(candidate.candidate)) continue;

			yield return new KeyValuePair<WordCandidate, bool>(candidate, false);
		}

		var rangeSeed = range[0];
		var rangeCandidate = horizontal ? HorizontalCandidate(rangeSeed) : VerticalCandidate(rangeSeed);

		if (!String.IsNullOrEmpty(rangeCandidate.candidate))
        {
			yield return new KeyValuePair<WordCandidate, bool>(rangeCandidate, true);
        }
	}
}
