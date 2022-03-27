using CsharpIngredientPhraseTagger.Training;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CsharpIngredientPhraseTagger.Tests
{
    public class TestTranslator
    {
        [Fact(DisplayName = "Translates row with simple phrase")]
        public void TestTranslatesRowWithSimplePhrase()
        {
            var row = new Ingredient
            {
                Input = "2 cups flour",
                Name = "flour",
                Quantity = 2.0f,
                RangeEnd = 0.0f,
                Unit = "cup",
                Comment = ""
            };

            var expected = "2\tI1\tL4\tNoCAP\tNoPAREN\tB-QTY\n" +
                           "cups\tI2\tL4\tNoCAP\tNoPAREN\tB-UNIT\n" +
                           "flour\tI3\tL4\tNoCAP\tNoPAREN\tB-NAME";
            var actual = Translator.TranslateRow(row).Trim();

            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "Translates row with simple fraction")]
        public void TestTranslatesRowWithSimpleFraction()
        {
            var row = new Ingredient
            {
                Input = "1/2 cup yellow cornmeal",
                Name = "yellow cornmeal",
                Quantity = 0.5f,
                RangeEnd = 0.0f,
                Unit = "cup",
                Comment = ""
            };

            var expected = "1/2\tI1\tL8\tNoCAP\tNoPAREN\tB-QTY\n" +
                           "cup\tI2\tL8\tNoCAP\tNoPAREN\tB-UNIT\n" +
                           "yellow\tI3\tL8\tNoCAP\tNoPAREN\tB-NAME\n" +
                           "cornmeal\tI4\tL8\tNoCAP\tNoPAREN\tI-NAME";

            var actual = Translator.TranslateRow(row).Trim();

            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "Translates row with complex fraction")]
        public void TestTranslatesRowWithComplexFraction()
        {
            var row = new Ingredient
            {
                Input = "1 1/2 teaspoons salt",
                Name = "salt",
                Quantity = 1.5f,
                RangeEnd = 0.0f,
                Unit = "teaspoon",
                Comment = ""
            };

            var expected = "1$1/2\tI1\tL4\tNoCAP\tNoPAREN\tB-QTY\n" +
                           "teaspoons\tI2\tL4\tNoCAP\tNoPAREN\tB-UNIT\n" +
                           "salt\tI3\tL4\tNoCAP\tNoPAREN\tB-NAME";

            var actual = Translator.TranslateRow(row).Trim();

            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "Translates row with non-ASCII characters")]
        public void TestTranslatesRowWithNonAsciiCharacters()
        {
            var row = new Ingredient
            {
                Input = "2 to 3 teaspoons minced jalape\xc3\xb1o",
                Name = "jalape\xc3\xb1os",
                Quantity = 2.0f,
                RangeEnd = 3.0f,
                Unit = "teaspoon",
                Comment = "minced"
            };

            var expected = "2\tI1\tL8\tNoCAP\tNoPAREN\tB-QTY\n" +
                           "to\tI2\tL8\tNoCAP\tNoPAREN\tOTHER\n" +
                           "3\tI3\tL8\tNoCAP\tNoPAREN\tB-RANGE_END\n" +
                           "teaspoons\tI4\tL8\tNoCAP\tNoPAREN\tB-UNIT\n" +
                           "minced\tI5\tL8\tNoCAP\tNoPAREN\tB-COMMENT\n" +
                           "jalape\xc3\xb1o\tI6\tL8\tNoCAP\tNoPAREN\tB-NAME";

            var actual = Translator.TranslateRow(row).Trim();

            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "Translates row with comment")]
        public void TestTranslatesRowWithComment()
        {
            var row = new Ingredient
            {
                Input = "Half a vanilla bean, split lengthwise, seeds scraped",
                Name = "vanilla bean",
                Quantity = 0.5f,
                RangeEnd = 0.0f,
                Unit = "",
                Comment = "split lengthwise, seeds scraped"
            };

            var expected = "Half\tI1\tL12\tYesCAP\tNoPAREN\tOTHER\n" +
                           "a\tI2\tL12\tNoCAP\tNoPAREN\tOTHER\n" +
                           "vanilla\tI3\tL12\tNoCAP\tNoPAREN\tB-NAME\n" +
                           "bean\tI4\tL12\tNoCAP\tNoPAREN\tI-NAME\n" +
                           ",\tI5\tL12\tNoCAP\tNoPAREN\tB-COMMENT\n" +
                           "split\tI6\tL12\tNoCAP\tNoPAREN\tI-COMMENT\n" +
                           "lengthwise\tI7\tL12\tNoCAP\tNoPAREN\tI-COMMENT\n" +
                           ",\tI8\tL12\tNoCAP\tNoPAREN\tI-COMMENT\n" +
                           "seeds\tI9\tL12\tNoCAP\tNoPAREN\tI-COMMENT\n" +
                           "scraped\tI10\tL12\tNoCAP\tNoPAREN\tI-COMMENT";

            var actual = Translator.TranslateRow(row).Trim();

            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "Translates complex row")]
        public void TestTranslatesComplexRow()
        {
            var row = new Ingredient
            {
                Input = "1 1/4 cups cooked and pureed fresh butternut squash, " +
                        "or 1 10-ounce package frozen squash, defrosted",
                Name = "butternut squash",
                Quantity = 1.25f,
                RangeEnd = 0.0f,
                Unit = "cup",
                Comment = "cooked and pureed fresh, or 1 10-ounce package " +
                          "frozen squash, defrosted"
            };

            var expected = "1$1/4\tI1\tL20\tNoCAP\tNoPAREN\tB-QTY\n" +
                           "cups\tI2\tL20\tNoCAP\tNoPAREN\tB-UNIT\n" +
                           "cooked\tI3\tL20\tNoCAP\tNoPAREN\tB-COMMENT\n" +
                           "and\tI4\tL20\tNoCAP\tNoPAREN\tI-COMMENT\n" +
                           "pureed\tI5\tL20\tNoCAP\tNoPAREN\tI-COMMENT\n" +
                           "fresh\tI6\tL20\tNoCAP\tNoPAREN\tI-COMMENT\n" +
                           "butternut\tI7\tL20\tNoCAP\tNoPAREN\tB-NAME\n" +
                           "squash\tI8\tL20\tNoCAP\tNoPAREN\tI-NAME\n" +
                           ",\tI9\tL20\tNoCAP\tNoPAREN\tOTHER\n" +
                           "or\tI10\tL20\tNoCAP\tNoPAREN\tI-COMMENT\n" +
                           "1\tI11\tL20\tNoCAP\tNoPAREN\tI-COMMENT\n" +
                           "10-ounce\tI12\tL20\tNoCAP\tNoPAREN\tI-COMMENT\n" +
                           "package\tI13\tL20\tNoCAP\tNoPAREN\tI-COMMENT\n" +
                           "frozen\tI14\tL20\tNoCAP\tNoPAREN\tI-COMMENT\n" +
                           "squash\tI15\tL20\tNoCAP\tNoPAREN\tB-NAME\n" +
                           ",\tI16\tL20\tNoCAP\tNoPAREN\tOTHER\n" +
                           "defrosted\tI17\tL20\tNoCAP\tNoPAREN\tI-COMMENT";

            var actual = Translator.TranslateRow(row).Trim();

            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "Translates row with multiple ingredients")]
        public void TestTranslatesRowWithMultipleIngredients()
        {
            var row = new Ingredient
            {
                Input = "4 to 6 tablespoons fresh lime juice, as needed, plus " +
                        "4 to 6 slices of lime, for garnish",
                Name = "fresh lime juice, as needed, plus 4 to 6 slices of " +
                       "lime, for garnish",
                Quantity = 4.0f,
                RangeEnd = 6.0f,
                Unit = "tablespoon",
                Comment = ""
            };

            var expected = "4\tI1\tLX\tNoCAP\tNoPAREN\tB-NAME\n" +
                           "to\tI2\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           "6\tI3\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           "tablespoons\tI4\tLX\tNoCAP\tNoPAREN\tB-UNIT\n" +
                           "fresh\tI5\tLX\tNoCAP\tNoPAREN\tB-NAME\n" +
                           "lime\tI6\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           "juice\tI7\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           ",\tI8\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           "as\tI9\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           "needed\tI10\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           ",\tI11\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           "plus\tI12\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           "4\tI13\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           "to\tI14\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           "6\tI15\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           "slices\tI16\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           "of\tI17\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           "lime\tI18\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           ",\tI19\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           "for\tI20\tLX\tNoCAP\tNoPAREN\tI-NAME\n" +
                           "garnish\tI21\tLX\tNoCAP\tNoPAREN\tI-NAME";

            var actual = Translator.TranslateRow(row).Trim();

            Assert.Equal(expected, actual);
        }
    }
}
