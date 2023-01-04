using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotHelpers;

namespace LanguageTools
{
    static class Lexicon
    {
        public class InitializationException : System.Exception {}

        public static string InitStatus { get; private set; }

        private enum LoadingState { Unloaded, Inizializing, Loaded };

        static private HashSet<string> lexicon = new HashSet<string>();

        static private int TriToBiRatio = 3;

        static private GramBag bigrams;
        static private GramBag trigrams;

        static private RandomNumberGenerator RNG;
        static private LoadingState loadingState = LoadingState.Unloaded;

        public static bool Unloaded
        {
            get
            {
                return loadingState == LoadingState.Unloaded;
            }
        }

        public static bool Inizializing
        {
            get
            {
                return loadingState == LoadingState.Inizializing;
            }
        }

        public static bool Loaded
        {
            get
            {
                return loadingState == LoadingState.Loaded;
            }
        }

        public static int Size
        {
            get
            {
                return lexicon?.Count() ?? 0;
            }
        }

        private static File resourceFile;
        private static int maxWordLength;

        public static void Init(File resourceFile, int maxLength)
        {
            if (loadingState == LoadingState.Inizializing) throw new InitializationException();

            InitStatus = "Preparing";
            loadingState = LoadingState.Inizializing;
            RNG = new RandomNumberGenerator();
            RNG.Randomize();

            Lexicon.resourceFile = resourceFile;
            maxWordLength = maxLength;
        }

        private static List<string> LoadWords()
        {
            SimpleTimer.Start("Load Words");
            var words = resourceFile.GetAsText().Split("\n").Select(w => w.Trim()).Where(w => w.Length > 0 && w.Length <= maxWordLength).ToList();
            lexicon.UnionWith(words);
            SimpleTimer.Stop("Load Words");
            return words;
        }

        private static IEnumerator<bool> LoadWordBatch()
        {
            SimpleTimer.Start("Load Words");
            InitStatus = "Reading in words";
            int batchSize = 20000;
            int i = 0;
            var wordList = new List<string>();
            while (!resourceFile.EofReached())
            {
                var line = resourceFile
                    .GetLine()
                    .Trim();

                i++;
                if (i >= batchSize) {
                    i = 0;
                    SimpleTimer.Stop("Load Words");
                    yield return true;
                    SimpleTimer.Start("Load Words");
                }

                if (line.Length == 0 || line.Length > maxWordLength) continue;
                wordList.Add(line);
            }
            resourceFile.Close();
            resourceFile = null;
            SimpleTimer.Stop("Load Words");
            yield return true;

            SimpleTimer.Start("Store Words");
            InitStatus = "Storing words";
            lexicon.UnionWith(wordList);
            wordList = null;
            SimpleTimer.Start("Store Words");
        }

        private static IEnumerator<bool> wordLoader;
        private static IEnumerator<Dictionary<string, int>> gramMaker;

        public static void LoadAll()
        {
            var words = LoadWords();

            SimpleTimer.Start("Load Bigrams");
            bigrams = new GramBag(words.UnbatchedCreateGrams());
            SimpleTimer.Stop("Load Bigrams");

            SimpleTimer.Start("Load Trigrams");
            trigrams = new GramBag(words.UnbatchedCreateGrams(3));
            SimpleTimer.Stop("Load Trigrams");

            loadingState = LoadingState.Loaded;
        }

        public static bool LoadBatch() {
            if (wordLoader == null)
            {
                wordLoader = LoadWordBatch();
                return false;
            } else if (wordLoader.MoveNext())
            {
                return false;
            }
            else if (bigrams == null)
            {   
                wordLoader.Dispose();

                SimpleTimer.Start("Load Bigrams");
                InitStatus = "Creating 2-Letter Blocks";
                if (gramMaker == null)
                {
                    gramMaker = lexicon.CreateGrams();
                }
                else if (gramMaker.MoveNext() && gramMaker.Current != null)
                {
                    bigrams = new GramBag(gramMaker.Current);
                    gramMaker.Dispose();
                    gramMaker = null;
                }
                SimpleTimer.Stop("Load Bigrams");
                return false;
            }
            else if (trigrams == null)
            {
                SimpleTimer.Start("Load Trigrams");
                InitStatus = "Creating 3-Letter Blocks";
                if (gramMaker == null)
                {
                    gramMaker = lexicon.CreateGrams(3);
                } else if (gramMaker.MoveNext() && gramMaker.Current != null)
                {
                    trigrams = new GramBag(gramMaker.Current);
                    gramMaker = null;
                    loadingState = LoadingState.Loaded;
                    return true;
                }
                SimpleTimer.Stop("Load Trigrams");
            }            
            return true;
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