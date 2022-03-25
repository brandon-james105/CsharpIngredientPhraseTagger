﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsharpIngredientPhraseTagger.Training
{
    public class Translator
    {
        /// <summary>
        ///     Translates a row of labeled data into CRF++-compatible tag strings.
        ///     
        ///     Args:
        ///         row: A row of data from the input CSV of labeled ingredient data.
        ///     
        ///     Returns:
        ///         The row of input converted to CRF++-compatible tags, e.g.
        ///     
        ///             2\tI1\tL4\tNoCAP\tNoPAREN\tB-QTY
        ///             cups\tI2\tL4\tNoCAP\tNoPAREN\tB-UNIT
        ///             flour\tI3\tL4\tNoCAP\tNoPAREN\tB-NAME
        /// </summary>
        public static string TranslateRow(Dictionary<string, string> row)
        {
            // extract the display name
            var displayInput = Utils.CleanUnicodeFractions(row["input"]);
            var tokens = Tokenizer.Tokenize(displayInput).ToHashSet();
            var labels = RowToLabels(row);
            var labelData = AddPrefixes(tokens.Select(t => new Tuple<string, List<string>>(t, MatchUp(t, labels))).ToList());
            var translated = "";

            for (int i = 0; i < labelData.Count; i++)
            {
                var token = labelData.ElementAt(i).Item1;
                var tags = labelData.ElementAt(i).Item2;
                var features = Utils.GetFeatures(token, i + 1, tokens);
                translated += Utils.JoinLine(features.Append(token).Append(BestTag(tags)).Append("\n"));
            }

            return translated;
        }

        /// <summary>
        ///     Extracts labels from a labelled ingredient data row.
        ///     
        ///     Args:
        ///         A row of full data about an ingredient, including input and labels.
        ///     
        ///     Returns:
        ///         A dictionary of the label data extracted from the row.
        /// </summary>
        public static Dictionary<string, string> RowToLabels(Dictionary<string, string> row)
        {
            var labels = new Dictionary<string, string> { };
            var labelKeys = new List<string> { "name", "qty", "range_end", "unit", "comment" };
            foreach (var key in labelKeys)
            {
                labels[key] = row[key];
            }
            return labels;
        }

        ///
        /// <summary>
        ///     Parses a string that represents a number into a decimal data type so that
        ///     we can match the quantity field in the db with the quantity that appears
        ///     in the display name. Rounds the result to 2 places.
        /// </summary>
        /// 
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

            var m2 = Regex.Match(@"^(\d)/(\d)$", ss);

            if (m2.Success)
            {
                num = float.Parse(m2.Groups.Values.ElementAt(1).Value) / float.Parse(m2.Groups.Values.ElementAt(2).Value);
                return decimal.Parse(Math.Round(num, 2).ToString());
            }

            return null;
        }

        /// 
        /// <summary>
        ///     Returns our best guess of the match between the tags and the
        ///     words from the display text.
        /// 
        ///     This problem is difficult for the following reasons:
        ///         * not all the words in the display name have associated tags
        ///         * the quantity field is stored as a number, but it appears
        ///           as a string in the display name
        ///         * the comment is often a compilation of different comments in
        ///           the display name
        /// </summary>
        ///     
        public static List<string> MatchUp(string token, Dictionary<string, string> labels)
        {
            var ret = new List<string>();
            // strip parens from the token, since they often appear in the
            // display_name, but are removed from the comment.
            token = Utils.NormalizeToken(token);
            var decimalToken = ParseNumbers(token);
            // Iterate through the labels in descending order of label importance.
            foreach (var labelKey in new List<string> { "name", "unit", "qty", "comment", "range_end" })
            {
                var labelValue = labels[labelKey];
                if (labelValue is string)
                {
                    foreach (var tuple1 in Tokenizer.Tokenize(labelValue).Select((_p_1, _p_2) => Tuple.Create(_p_2, _p_1)))
                    {
                        var n = tuple1.Item1;
                        var vt = tuple1.Item2;
                        if (Utils.NormalizeToken(vt) == token)
                        {
                            ret.Add(labelKey.ToUpper());
                        }
                    }
                }
                else if (decimalToken != null)
                {
                    if (labelValue == decimalToken.ToString())
                    {
                        ret.Add(labelKey.ToUpper());
                    }
                }
            }
            return ret;
        }

        /// <summary>
        ///     We use BIO tagging/chunking to differentiate between tags
        ///     at the start of a tag sequence and those in the middle. This
        ///     is a common technique in entity recognition.
        /// 
        ///     Reference: http://www.kdd.cis.ksu.edu/Courses/Spring-2013/CIS798/Handouts/04-ramshaw95text.pdf
        /// </summary>
        public static List<Tuple<string, List<string>>> AddPrefixes(List<Tuple<string, List<string>>> data)
        {
            IEnumerable<string>? prevTags = null;
            var newData = new List<Tuple<string, List<string>>>();

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
                newData.Add(new Tuple<string, List<string>>(token, newTags));
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

