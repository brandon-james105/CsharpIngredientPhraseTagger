using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsharpIngredientPhraseTagger
{
    public static class Tokenizer
    {
        /// <summary>
        /// Replaces the whitespace between the integer and fractional part of a quantity
        /// with a dollar sign, so it's interpreted as a single token. The rest of the
        /// string is left alone.
        /// 
        ///     clumpFractions("aaa 1 2/3 bbb")
        ///     # => "aaa 1$2/3 bbb"
        /// </summary>
        ///     
        public static string ClumpFractions(string s)
        {
            return Regex.Replace(s, @"(\d+)\s+(\d)/(\d)", "$1$$$2/$3");
        }

        /// <summary>
        /// Tokenize on parenthesis, punctuation, spaces and American units followed by a slash.
        /// 
        /// We sometimes give American units and metric units for baking recipes. For example:
        ///     * 2 tablespoons/30 mililiters milk or cream
        ///     * 2 1/2 cups/300 grams all-purpose flour
        /// 
        /// The recipe database only allows for one unit, and we want to use the American one.
        /// But we must split the text on "cups/" etc. in order to pick it up.
        /// </summary>
        ///     
        public static IEnumerable<string> Tokenize(string s)
        {
            // handle abbreviation like "100g" by treating it as "100 grams"
            s = Regex.Replace(s, @"(\d+)g", "$1 grams");
            s = Regex.Replace(s, @"(\d+)oz", "$1 ounces");
            s = Regex.Replace(s, @"(\d+)ml", "$1 milliliters", RegexOptions.IgnoreCase);
            var americanUnits = new string[] { "cup", "tablespoon", "teaspoon", "pound", "ounce", "quart", "pint" };
            
            // The following removes slashes following American units and replaces it with a space.
            foreach (var unit in americanUnits)
            {
                s = s.Replace(unit + "/", unit + " ");
                s = s.Replace(unit + "s/", unit + "s ");
            }

            var tokens = from token in Regex.Split(ClumpFractions(s), @"([,()\s]{1})")
                            where token.Length > 0 && token.Trim().Length > 0
                            select token.Trim();
            return tokens.ToArray();
        }
    }
}
