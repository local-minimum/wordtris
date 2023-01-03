using System.Collections.Generic;
using System.Linq;
using Godot;
using LanguageTools;

namespace LanguageTools
{
    class Lexicon
    {
        private HashSet<string> lexicon = new HashSet<string>();

        private int TriToBiRatio = 3;

        private GramBag bigrams;
        private GramBag trigrams;

        private RandomNumberGenerator RNG;

        public int Size
        {
            get
            {
                return lexicon.Count();
            }
        }

        public Lexicon(string resource, int maxLength)
        {
            RNG = new RandomNumberGenerator();
            RNG.Randomize();

            var wordList = resource
                .LoadResource()
                .Where(w => w.Length <= maxLength)
                .ToList();

            lexicon.UnionWith(wordList);

            Dictionary<string, int> bigramsDict = new Dictionary<string, int>();
            Dictionary<string, int> trigramsDict = new Dictionary<string, int>();
            wordList.CreateGrams(bigramsDict);
            wordList.CreateGrams(trigramsDict, 3);
            bigrams = new GramBag(bigramsDict);
            trigrams = new GramBag(trigramsDict);

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

        public string LongestWordAround(string input, int newCharacterPosition, int minLength = 2)
        {
            return input
                .ToUpper()
                .SubstringsAround(newCharacterPosition, minLength)
                .OrderBy(w => -w.Length)
                .Where(w => lexicon.Contains(w))
                .FirstOrDefault();
        }

        public string LongestWordAroundRange(string input, int firstNewCharacterPosition, int lastNewCharacterPosition, int minLength)
        {
            return input
                .ToUpper()
                .SubstringsAroundRange(firstNewCharacterPosition, lastNewCharacterPosition, minLength)
                .OrderBy(w => -w.Length)
                .Where(w => lexicon.Contains(w))
                .FirstOrDefault();
        }

        public bool isWord(string candidate) => lexicon.Contains(candidate.ToUpper());
    }
}