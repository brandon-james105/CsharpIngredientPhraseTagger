using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsharpIngredientPhraseTagger.Training
{
    public static class Utils
    {
        public static string JoinLine(IEnumerable<string> columns)
        {
            return string.Join("\t", columns);
        }

        /// <summary>
        /// Replace unicode fractions with ascii representation, preceded by a
        /// space.
        ///
        /// "1\x215e" => "1 7/8"
        /// </summary>
        public static string CleanUnicodeFractions(string s)
        {
            var fractions = new Dictionary<string, string> {
                { "\x215b", "1/8" },
                { "\x215c", "3/8" },
                { "\x215d", "5/8" },
                { "\x215e", "7/8" },
                { "\x2159", "1/6" },
                { "\x215a", "5/6" },
                { "\x2155", "1/5" },
                { "\x2156", "2/5" },
                { "\x2157", "3/5" },
                { "\x2158", "4/5" },
                { "\xbc", " 1/4" },
                { "\xbe", "3/4" },
                { "\x2153", "1/3" },
                { "\x2154", "2/3" },
                { "\xbd", "1/2"}
            };

            foreach (var fraction in fractions)
            {
                var unicodeFraction = fraction.Key;
                var asciiFraction = fraction.Value;
                s = s.Replace(unicodeFraction, " " + asciiFraction);
            }
            return s;
        }

        ///
        /// <summary>
        /// Replacess $'s with spaces. The reverse of clumpFractions.
        /// </summary>
        ///     
        public static string Unclump(string s)
        {
            return s.Replace("$", " ");
        }

        // 
        //     To do: FIX THIS. We used to use the pattern.en package to singularize words, but
        //     in the name of simple deployments, we took it out. We should fix this at some
        //     point.
        //     
        public static string NormalizeToken(string s) => Singularize(s);

        ///
        /// <summary>
        /// Returns a list of features for a given token.
        /// </summary>
        ///     
        public static IEnumerable<string> GetFeatures(string token, int index, IEnumerable<string> tokens)
        {
            var length = tokens.Count();
            return new string[] {
                string.Format("I{0}", index),
                string.Format("L{0}", LengthGroup(length)),
                (IsCapitalized(token) ? "Yes" : "No") + "CAP",
                (InsideParenthesis(token, tokens) ? "Yes" : "No") + "PAREN"
            };
        }

        ///
        /// <summary>
        /// Uses Humanizr to change the quantity of a word to singular
        /// </summary>
        ///     
        public static string Singularize(string word)
        {
            return word.ToQuantity(1, ShowQuantityAs.None);
        }

        ///<summary>
        /// Returns true if a given token starts with a capital letter.
        ///</summary>
        ///     
        public static bool IsCapitalized(string token)
        {
            var isCaps = Regex.IsMatch(token, @"^[A-Z]");
            return isCaps;
        }

        ///
        /// <summary>
        /// Buckets the length of the ingredient into 6 buckets.
        /// </summary>
        ///     
        public static string LengthGroup(int actualLength)
        {
            foreach (var n in new List<int> { 4, 8, 12, 16, 20 })
            {
                if (actualLength < n)
                {
                    return n.ToString();
                }
            }
            return "X";
        }

        ///
        /// <summary>
        /// Returns true if the word is inside parenthesis in the phrase.
        /// </summary>
        ///     
        public static bool InsideParenthesis(string token, IEnumerable<string> tokens)
        {
            if (token.StartsWith("(") && token.EndsWith(")"))
            {
                return true;
            }
            else
            {
                var line = string.Join(" ", tokens);
                var pattern = @".*\(.*" + Regex.Escape(token) + @".*\).*";
                var isMatch = Regex.Match(pattern, line).Success;
                return isMatch;
            }
        }

        /// 
        /// <summary>
        /// Format a list of (tag, [tokens]) tuples as an HTML string for display.
        /// 
        /// displayIngredient([("qty", ["1"]), ("name", ["cat", "pie"])])
        /// # => <span class='qty'>1</span> <span class='name'>cat pie</span>
        /// </summary>
        ///
        public static string DisplayIngredient(IDictionary<string, List<string>> ingredient)
        {
            var tokenDisplay = (from _tup_1 in ingredient
                                let tag = _tup_1.Key
                                let tokens = _tup_1.Value
                                select string.Format("<span class=\"{0}\">{1}</span>", tag, string.Join(" ", tokens))).ToList();
            return string.Join("", tokenDisplay);
        }

        // HACK: fix this
        ///
        /// <summary>
        /// Joins list of words with spaces, but is smart about not adding spaces
        /// before commas.
        /// </summary>
        ///
        public static string SmartJoin(IEnumerable<string> words)
        {
            var input = string.Join(" ", words);
            // replace " , " with ", "
            input = input.Replace(" , ", ", ");
            // replace " ( " with " ("
            input = input.Replace("( ", "(");
            // replace " ) " with ") "
            input = input.Replace(" )", ")");
            return input;
        }

        ///
        /// <summary>
        /// This takes the output of CRF++ and turns it into an actual
        /// data structure.
        /// </summary>
        ///
        public static List<Dictionary<string, string>> ImportData(IEnumerable<string> lines)
        {
            var data = new List<Dictionary<string, List<string>>>();
            var display = new List<List<Tuple<string, List<string>>>>();
            data.Add(new Dictionary<string, List<string>>());
            display.Add(new List<Tuple<string, List<string>>>());
            string? prevTag = null;
            //
            // iterate lines in the data file, which looks like:
            //
            //   # 0.511035
            //   1/2       I1  L12  NoCAP  X  B-QTY/0.982850
            //   teaspoon  I2  L12  NoCAP  X  B-UNIT/0.982200
            //   fresh     I3  L12  NoCAP  X  B-COMMENT/0.716364
            //   thyme     I4  L12  NoCAP  X  B-NAME/0.816803
            //   leaves    I5  L12  NoCAP  X  I-NAME/0.960524
            //   ,         I6  L12  NoCAP  X  B-COMMENT/0.772231
            //   finely    I7  L12  NoCAP  X  I-COMMENT/0.825956
            //   chopped   I8  L12  NoCAP  X  I-COMMENT/0.893379
            //
            //   # 0.505999
            //   Black   I1  L8  YesCAP  X  B-NAME/0.765461
            //   pepper  I2  L8  NoCAP   X  I-NAME/0.756614
            //   ,       I3  L8  NoCAP   X  OTHER/0.798040
            //   to      I4  L8  NoCAP   X  B-COMMENT/0.683089
            //   taste   I5  L8  NoCAP   X  I-COMMENT/0.848617
            //
            // i.e. the output of crf_test -v 1
            //
            foreach (var line in lines)
            {
                // blank line starts a new ingredient
                if (line.Equals("\n") || line.Equals(""))
                {
                    data.Add(new Dictionary<string, List<string>>());
                    display.Add(new List<Tuple<string, List<string>>>());
                    prevTag = null;
                }
                else if (line[0] == '#')
                {
                    continue;
                }
                else
                {
                    var columns = Regex.Split(line.Trim(), "\t");
                    var token = Unclump(columns.First().Trim());
                    var tagAndConfidence = Regex.Split(columns.Last(), "/");
                    var tag = tagAndConfidence.First();
                    var confidence = tagAndConfidence.Last();
                    tag = Regex.Replace(tag, @"[BI]\-", "").ToLower();

                    if (prevTag != tag)
                    {
                        var lastDisplay = display.Last();
                        lastDisplay.Add(new Tuple<string, List<string>>(tag, new List<string>() { token }));
                        prevTag = tag;
                    }
                    else
                    {
                        display.Last().Last().Item2.Add(token);
                    }

                    if (!data.Last().ContainsKey(tag))
                    {
                        data.Last()[tag] = new List<string>();
                    }

                    if (tag == "unit")
                    {
                        token = Singularize(token);
                    }

                    data.Last()[tag].Add(token);
                }
            }

            var output = data.Select((ingredient, index) =>
            {
                var dictionary = ingredient.ToDictionary(i => i.Key, i => SmartJoin(i.Value));
                dictionary["display"] = DisplayIngredient(ingredient);
                dictionary["input"] = SmartJoin(display[index].Select(i => string.Join(" ", i.Item2)));
                return dictionary;
            }).ToList();

            return output;
        }

        /// <summary>
        /// Parse "raw" ingredient lines into CRF-ready output
        /// </summary> 
        public static string ExportData(IEnumerable<string> lines)
        {
            var output = new List<string>();
            foreach (var line in lines)
            {
                var lineClean = Regex.Replace(line, "<[^<]+?>", "");
                var tokens = Tokenizer.Tokenize(lineClean);

                for (int i = 0; i < tokens.Count(); i++)
                {
                    var token = tokens.ElementAt(i);
                    var features = GetFeatures(token, i + 1, tokens);
                    output.Add(JoinLine(features.Prepend(token)));
                }

                output.Append("");
            }
            return string.Join("\n", output);
        }
    }
}