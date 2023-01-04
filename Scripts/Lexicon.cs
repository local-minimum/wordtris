using System.Collections.Generic;
using System.Linq;
using Godot;

namespace LanguageTools
{
    static class Lexicon
    {
        static private HashSet<string> lexicon = new HashSet<string>();

        static private int TriToBiRatio = 3;

        static private GramBag bigrams;
        static private GramBag trigrams;

        static private RandomNumberGenerator RNG;

        public static bool Initizialized
        {
            get
            {
                return bigrams != null;
            }
        }

        public static int Size
        {
            get
            {
                return lexicon.Count();
            }
        }

        public static void Init(string resourceData, int maxLength)
        {
            RNG = new RandomNumberGenerator();
            RNG.Randomize();

            var wordList = resourceData
                .AsCleanListOfRows()
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

        public static string RandomGram
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

        public static string LongestWordAround(string input, int newCharacterPosition, int minLength = 2)
        {
            return input
                .ToUpper()
                .SubstringsAround(newCharacterPosition, minLength)
                .OrderBy(w => -w.Length)
                .Where(w => lexicon.Contains(w))
                .FirstOrDefault();
        }

        public static string LongestWordAroundRange(string input, int firstNewCharacterPosition, int lastNewCharacterPosition, int minLength)
        {
            return input
                .ToUpper()
                .SubstringsAroundRange(firstNewCharacterPosition, lastNewCharacterPosition, minLength)
                .OrderBy(w => -w.Length)
                .Where(w => lexicon.Contains(w))
                .FirstOrDefault();
        }

        public static bool isWord(string candidate) => lexicon.Contains(candidate.ToUpper());
    }
}