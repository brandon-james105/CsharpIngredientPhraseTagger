using CsharpIngredientPhraseTagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CsharpIngredientPhraseTagger.Tests
{
    public class TestTokenizer
    {
        [Fact(DisplayName = "Tokenizer splits slash separated alternatives")]
        public void TestTokenizerSplitsSlashSeparatedAlternative()
        {
            var pairs = new List<Tuple<string, IEnumerable<string>>>()
            {
                new Tuple<string, IEnumerable<string>>("2 tablespoons/30 milliliters milk or cream",
                                                new string[] { "2", "tablespoons", "30", "milliliters", "milk", "or", "cream" }),

                new Tuple<string, IEnumerable<string>>("2 1/2 cups/300 grams all-purpose flour",
                                                new string[] { "2$1/2", "cups", "300", "grams", "all-purpose", "flour" })
            };

            foreach (var (ingredient, tokensExpected) in pairs)
            {
                var tokensActual = Tokenizer.Tokenize(ingredient);
                Assert.Equal(tokensExpected, tokensActual);
            }
        }

        [Fact(DisplayName = "Tokenizer parens")]
        public void TestTokenizerParens()
        {
            var expected = new string[] { "2", "tablespoons", "milk", "(", "or", "cream", ")" };
            var actual = Tokenizer.Tokenize("2 tablespoons milk (or cream)");

            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "Tokenizer commas")]
        public void TestTokenizerCommas()
        {
            var expected = new string[] { "Half", "a", "vanilla", "bean", ",", "split", "lengthwise", ",", "seeds", "scraped" };
            var actual = Tokenizer.Tokenize("Half a vanilla bean, split lengthwise, seeds scraped");

            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "Tokenizer parens and commas")]
        public void TestTokenizerParensAndCommas()
        {
            var expected = new string[] { "1", "cup", "peeled", "and", "cooked", "fresh", "chestnuts", "(",
                                          "about", "20", ")", ",", "or", "1", "cup", "canned", ",",
                                          "unsweetened", "chestnuts" };
            var actual = Tokenizer.Tokenize("1 cup peeled and cooked fresh chestnuts (about 20), or 1 cup canned, unsweetened chestnuts");

            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "Tokenizer expands unit abbreviations")]
        public void TestTokenizerExpandsUnitAbbreviations()
        {
            var pairs = new List<Tuple<string, IEnumerable<string>>>()
            {
                new Tuple<string, IEnumerable<string>>("100g melted chocolate", new string[] { "100", "grams", "melted", "chocolate" }),
                new Tuple<string, IEnumerable<string>>("8oz diet coke", new string[] { "8", "ounces", "diet", "coke" }),
                new Tuple<string, IEnumerable<string>>("15ml coconut oil", new string[] { "15", "milliliters", "coconut", "oil" }),
                new Tuple<string, IEnumerable<string>>("15mL coconut oil", new string[] { "15", "milliliters", "coconut", "oil" })
            };

            foreach (var (ingredient, tokensExpected) in pairs)
            {
                var actual = Tokenizer.Tokenize(ingredient);
                Assert.Equal(tokensExpected, actual);
            }
        }
    }
}
