using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LanguageTools
{
    public class GramBag
    {
        public int GramLength { get; private set; }
        public int Size { get; private set; }

        private string[] Grams;
        private int[] Frequencies;
        private int TotalFrequency;

        private RandomNumberGenerator RNG;

        public GramBag(Dictionary<string, int> grams)
        {
            Frequencies = grams.Values.ToArray();
            Grams = grams.Keys.ToArray();

            if (Frequencies.Length == 0) return;

            GramLength = Grams[0].Length;
            Size = Grams.Length;

            TotalFrequency = 0;
            for (int i = 1; i < Grams.Length; i++)
            {
                if (Grams[i].Length != GramLength)
                {
                    throw new System.ArgumentException(String.Format("All grams must be of equal length, {0} is not {1} characters long", Grams[i], GramLength));
                }
                TotalFrequency += Frequencies[i];
            }

            RNG = new RandomNumberGenerator();
            RNG.Randomize();
        }

        public bool Includes(string gram)
        {
            if (gram.Length != GramLength) return false;
            return Grams.Contains(gram);
        }

        public string RandomGram
        {
            get
            {
                return Grams[RNG.RandiRange(0, Size - 1)];
            }
        }

        public string WeightedRandomGram
        {
            get
            {
                var value = RNG.RandiRange(0, TotalFrequency - 1);
                for (int i = 0; i < Size; i++)
                {
                    var freq = Frequencies[i];
                    if (value < freq) return Grams[i];
                    value -= freq;
                }

                return null;
            }
        }
    }
}