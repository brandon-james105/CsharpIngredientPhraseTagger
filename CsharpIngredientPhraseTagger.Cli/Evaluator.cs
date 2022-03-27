using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsharpIngredientPhraseTagger.Exec
{
    public static class Evaluator
    {
        public static string Evaluate(string fileName)
        {
            var file = File.ReadAllText(fileName);
            var sentences = file.Split(Environment.NewLine);
            var totalSentences = sentences.Count();
            int totalWords = 0, correctWords = 0, correctSentences = 0;

            foreach (var sentence in sentences)
            {
                int correctWordsPerSentence = 0, totalWordsPerSentence = 0;

                foreach (var word in sentence.Split("\n"))
                {
                    var line = word.Trim().Split("\t");

                    if (line.Length > 1)
                    {
                        var currentWord = line[0];
                        var currentGuess = line[line.Length - 2];
                        var gold = line.Last();

                        if (!word.Trim().Contains(','))
                        {
                            totalWords++;
                            totalWordsPerSentence++;

                            if (currentGuess == gold
                                || currentGuess.Substring(2) == gold.Substring(2))
                            {
                                correctWords++;
                                correctWordsPerSentence++;
                            }
                        }
                    }
                }

                if (totalWordsPerSentence == correctWordsPerSentence)
                {
                    correctSentences++;
                }
            }

            return $"Sentence-level Stats\n" +
                   $"\tcorrect: {correctSentences}\n" +
                   $"\ttotal: {totalSentences}\n" +
                   $"\t% correct: {(double)correctSentences / totalSentences:P}\n" +
                   "\n" +
                   "Word-Level Stats:\n" +
                   $"\tcorrect: {correctWords}\n" +
                   $"\ttotal: {totalWords}\n" +
                   $"\t% correct: {(double)correctWords / totalWords:P}";
        }
    }
}
