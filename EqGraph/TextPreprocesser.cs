using Accord.MachineLearning.Text.Stemmers;
using System.Collections.Generic;
using System.Linq;

namespace EqGraph
{
    public class TextPreprocesser
    {
        static Dictionary<string, string> replacements = new Dictionary<string, string>()
        {
            { "1-го", "первого" },
            { "2-го", "второго" },
            { "'", "" },
            { ",", "" },
            { "уравнение", "" },
            { "уравнения", "" },
            { "уравнений", "" },
            { "общеее", "общее" }
        };
        public static string[] RemoveShortStrings(string[] arr)
        {
            return arr.Where(str => str.Length > 2).ToArray();
        }

        public static string MakeReplacements(string label, Dictionary<string, string> replacements)
        {
            foreach (var replacement in replacements)
            {
                label = label.Replace(replacement.Key, replacement.Value);
            }
            return label;
        }

        public static string[] ApplyStemming(string[] arr)
        {
            var stemmer = new RussianStemmer();
            var temp = arr
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(e => stemmer.Stem(e)).ToArray();
            return temp;
        }

        public static string[] ApplyPreprocess(string label)
        {
            label = MakeReplacements(label.ToLower(), replacements);
            var tokens = label.Split(' ', '-');
            tokens = RemoveShortStrings(tokens);
            tokens = ApplyStemming(tokens);
            return tokens;
        }
    }
}
