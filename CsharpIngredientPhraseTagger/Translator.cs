using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsharpIngredientPhraseTagger
{
    public class Translator
    {
        /// <summary>
        /// Translates a row of labeled data into CRF++-compatible tag strings.
        /// </summary>
        /// <param name="row">A row of data from the input CSV of labeled ingredient data.</param>
        /// <returns>
        /// The row of input converted to CRF++-compatible tags, e.g.
        /// <br/>
        /// <br/><code>2\tI1\tL4\tNoCAP\tNoPAREN\tB-QTY
        /// <br/>cups\tI2\tL4\tNoCAP\tNoPAREN\tB-UNIT
        /// <br/>flour\tI3\tL4\tNoCAP\tNoPAREN\tB-NAME</code>
        /// </returns>
        public static string TranslateRow(Ingredient row)
        {
            // extract the display name
            var displayInput = Utils.CleanUnicodeFractions(row.Input);
            var tokens = Tokenizer.Tokenize(displayInput);

            var labels = RowToLabels(row);
            var labelData = AddPrefixes(tokens.Select(t => new Tuple<string, IList<string>>(t, MatchUp(t, labels))).ToList());
            var translated = "";

            var i = 0;
            foreach (var (token, tags) in labelData)
            {
                var features = Utils.GetFeatures(token, ++i, tokens);
                translated += Utils.JoinLine(features.Prepend(token).Append(BestTag(tags))) + "\n";
            }

            return translated;
        }

        /// <summary>
        /// Extracts labels from a labelled ingredient data row.
        /// 
        /// Args:
        ///     A row of full data about an ingredient, including input and labels.
        /// 
        /// Returns:
        ///     A dictionary of the label data extracted from the row.
        /// </summary>
        public static Dictionary<string, object> RowToLabels(Ingredient row)
        {
            return new Dictionary<string, object>
            {
                { "input", row.Input },
                { "comment", row.Comment },
                { "name", row.Name },
                { "qty", row.Quantity },
                { "range_end", row.RangeEnd },
                { "unit", row.Unit }
            };
        }

        /// <summary>
        /// Parses a string that represents a number into a decimal data type so that
        /// we can match the quantity field in the db with the quantity that appears
        /// in the display name. Rounds the result to 2 places.
        /// </summary>
        public static decimal? ParseNumbers(string s)
        {
            double num;

            var ss = Utils.Unclump(s);
            var m3 = Regex.Match(ss, @"^\d+$");

            if (m3.Success)
            {
                return (decimal) Math.Round(float.Parse(ss), 2);
            }

            var m1 = Regex.Match(ss, @"(\d+)\s+(\d)/(\d)");

            if (m1.Success)
            {
                num = Convert.ToInt32(m1.Groups.Values.ElementAt(1).Value)
                    + float.Parse(m1.Groups.Values.ElementAt(2).Value) 
                    / float.Parse(m1.Groups.Values.ElementAt(3).Value);
                return decimal.Parse(Math.Round(num, 2).ToString());
            }

            var m2 = Regex.Match(ss, @"^(\d)/(\d)$");

            if (m2.Success)
            {
                num = float.Parse(m2.Groups.Values.ElementAt(1).Value) / float.Parse(m2.Groups.Values.ElementAt(2).Value);
                return decimal.Parse(Math.Round(num, 2).ToString());
            }

            return null;
        }

        /// <summary>
        /// Returns our best guess of the match between the tags and the
        /// words from the display text.
        /// 
        /// This problem is difficult for the following reasons:
        ///     * not all the words in the display name have associated tags
        ///     * the quantity field is stored as a number, but it appears
        ///       as a string in the display name
        ///     * the comment is often a compilation of different comments in
        ///       the display name
        /// </summary>
        public static IList<string> MatchUp(string token, IDictionary<string, object> labels)
        {
            var ret = new List<string>();
            // strip parens from the token, since they often appear in the
            // display_name, but are removed from the comment.
            token = Utils.NormalizeToken(token);
            var decimalToken = ParseNumbers(token);
            // Iterate through the labels in descending order of label importance.
            foreach (var labelKey in new string[] { "name", "unit", "qty", "comment", "range_end" })
            {
                var labelValue = labels[labelKey];
                if (labelValue is string)
                {
                    foreach (var (n, vt) in Tokenizer.Tokenize((string)labelValue).Select((_p_1, _p_2) => Tuple.Create(_p_2, _p_1)))
                    {
                        if (Utils.NormalizeToken(vt) == token)
                        {
                            ret.Add(labelKey.ToUpper());
                        }
                    }
                }
                else if (decimalToken != null)
                {
                    if (Convert.ToDecimal(labelValue) == decimalToken)
                    {
                        ret.Add(labelKey.ToUpper());
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// We use BIO tagging/chunking to differentiate between tags
        /// at the start of a tag sequence and those in the middle. This
        /// is a common technique in entity recognition.
        /// 
        /// Reference: http://www.kdd.cis.ksu.edu/Courses/Spring-2013/CIS798/Handouts/04-ramshaw95text.pdf
        /// </summary>
        public static IEnumerable<Tuple<string, IList<string>>> AddPrefixes(IEnumerable<Tuple<string, IList<string>>> data)
        {
            IEnumerable<string>? prevTags = null;
            var newData = new List<Tuple<string, IList<string>>>();

            foreach (var item in data)
            {
                var token = item.Item1;
                var tags = item.Item2;
                var newTags = new List<string>();
                foreach (var t in tags)
                {
                    var p = prevTags == null || !prevTags.Contains(t) ? "B" : "I";
                    newTags.Add(string.Format("{0}-{1}", p, t));
                }
                newData.Add(new Tuple<string, IList<string>>(token, newTags));
                prevTags = tags;
            }
            return newData;
        }

        public static string BestTag(IEnumerable<string> tags)
        {
            if (tags.Count() == 1)
            {
                return tags.ElementAt(0);
            }
            else
            {
                // if there are multiple tags, pick the first which isn't COMMENT
                foreach (var t in tags)
                {
                    if (t != "B-COMMENT" && t != "I-COMMENT")
                    {
                        return t;
                    }
                }
            }
            // we have no idea what to guess
            return "OTHER";
        }
    }
}

