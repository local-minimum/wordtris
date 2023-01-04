using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


namespace LanguageTools
{
    public static class LanguageExtentions
    {
        private static int batchSize = 2500;

        public static IEnumerable<string> Substrings(this IEnumerable<string> strings, int segmentSize)
        {
            foreach (var word in strings)
            {
                for (int j = 0, wl = word.Length - segmentSize; j <= wl; j++)
                {
                    yield return word.Substring(j, segmentSize);                    
                }
            }
        }

        public static Dictionary<string, int> UnbatchedCreateGrams(this List<string> words, int gramSize = 2)
        {
            Dictionary<string, int> grams = new Dictionary<string, int>();
            if (gramSize < 1) return grams;
            
            int nGrams = words
                .Where(w => w.Length >= gramSize)
                .Substrings(gramSize)
                .GroupBy(gram => gram)
                .Select(gramGroup => {
                    grams[gramGroup.First()] = gramGroup.Count();
                    return true;
                })
                .Count();

            GD.Print($"{nGrams} grams created of length {gramSize}");

            return grams;
        }

        public static IEnumerator<Dictionary<string, int>> CreateGrams(this HashSet<string> wordList, int gramSize = 2)
        {
            Dictionary<string, int> grams = new Dictionary<string, int>();

            if (gramSize < 1) {
                yield return grams;
                yield break;
            }

            int batch = 1;
            foreach(var word in wordList)
            {             
                batch++;
                if (batch % batchSize == 0)
                {                    
                    yield return null;
                }

                if (word.Length < gramSize)
                {
                    continue;
                }

                for (int j = 0, wl = word.Length - gramSize; j <= wl; j++)
                {
                    string gram = word.Substring(j, gramSize);
                    int count;
                    if (grams.TryGetValue(gram, out count))
                    {
                        grams[gram] = count + 1;
                    } else
                    {
                        grams[gram] = 1;
                    }
                }
            }

            yield return grams;
        }

        public static string DebugContent<K, V>(this Dictionary<K, V> store, int maxLength = -1)
        {
            var items = String.Join(", ", store.Select(kvp => String.Format("{0}: {1}", kvp.Key, kvp.Value)));
            if (maxLength > 0 && items.Length > maxLength)
            {
                items = String.Format("{0}...", items.Substring(0, maxLength));
            }

            return String.Format("<{0}>", items);
        }

        public static string DebugContent(this IEnumerable<string> strings, int maxLength = -1)
        {
            var items = String.Join(", ", strings);
            if (maxLength > 0 && items.Length > maxLength)
            {
                items = String.Format("{0}...", items.Substring(0, maxLength));
            }

            return String.Format("[{0}]", items);
        }

        public static IEnumerable<string> SubstringsAround(this string input, int position, int minLength = -1)
        {
            int start = 0;
            int end = input.Length;

            while (start < end && start <= position && end > position)
            {
                var l = end - start;
                if (minLength < 0 || l >= minLength)
                {
                    yield return input.Substring(start, l);
                }                

                start++;
                if (start > position)
                {
                    start = 0;
                    end--;

                    if (end < position) break;
                }
            }
        }

        public static IEnumerable<string> SubstringsAroundRange(this string input, int first, int last, int minLength = -1) {
            int start = 0;
            int end = input.Length;

            while (start < end && first < last && start <= last && end > first)
            {
                var l = end - start;
                if (minLength < 0 || l >= minLength)
                {
                    yield return input.Substring(start, end - start);
                }

                start++;
                if (start == end || start > last)
                {
                    start = 0;
                    end--;

                    if (end == first) break;
                }
            }
        }

        public static List<string> AsCleanListOfRows(this string resource)
        {
            return resource
                .Split(new string[] { "\n" }, StringSplitOptions.None)
                .Select(x => x.Trim())
                .ToList();
        }
    }
}
