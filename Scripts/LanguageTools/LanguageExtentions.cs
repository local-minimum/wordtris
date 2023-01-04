using System;
using System.Collections.Generic;
using System.Linq;


namespace LanguageTools
{
    public static class LanguageExtentions
    {
        private static int batchSize = 10000;

        public static IEnumerator<Dictionary<string, int>> CreateGrams(this IEnumerable<string> wordList, Dictionary<string, int> grams, int gramSize = 2)
        {
            if (gramSize < 1) {
                yield return grams;
                yield break;
            }

            int batch = 0;
            foreach (var word in wordList)
            {             
                batch++;
                if (batch >= batchSize)
                {
                    batch = 0;
                    yield return null;
                }

                if (word.Length < gramSize)
                {
                    continue;
                }

                for (int j = 0, wl = word.Length - gramSize; j <= wl; j++)
                {
                    var gram = word.Substring(j, gramSize);

                    if (grams.ContainsKey(gram))
                    {
                        grams[gram]++;
                    }
                    else
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
