using CsharpIngredientPhraseTagger.Training;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace CsharpIngredientPhraseTagger.Tests
{
    public class TestLabelledData
    {
        [Theory(DisplayName = "Reads Valid Label File")]
        [InlineData("index,input,name,qty,range_end,unit,comment\n" +
                    "63,4 to 6 large cloves garlic,garlic,4.0,6.0,clove,\n" +
                    "77,3 bananas,bananas,3.0,0.0,,\n" +
                    "106,\"2 1/2 pounds bell peppers (about 6 peppers in assorted colors), cut into 2-inch chunks\",bell peppers,2.5,0.0,pound,\"(about 6 peppers in assorted colors), cut into 2-inch chunks\"")]
        public void TestReadsValidLabelFile(string input)
        {
            var expected = new List<Ingredient>()
            {
                new Ingredient()
                {
                    Input = "4 to 6 large cloves garlic",
                    Quantity = 4.0f,
                    Unit = "clove",
                    Name = "garlic",
                    RangeEnd = 6.0f,
                    Comment = ""
                },
                new Ingredient()
                {
                    Input = "3 bananas",
                    Quantity = 3.0f,
                    Unit = "",
                    Name = "bananas",
                    Comment = "",
                    RangeEnd = 0.0f
                },
                new Ingredient()
                {
                    Input = "2 1/2 pounds bell peppers (about 6 peppers in assorted colors), cut into 2-inch chunks",
                    Quantity = 2.5f,
                    Unit = "pound",
                    Name = "bell peppers",
                    RangeEnd = 0.0f,
                    Comment = "(about 6 peppers in assorted colors), cut into 2-inch chunks"
                }
            };

            var reader = new Reader(new StringReader(input));
            var actual = new List<Ingredient>();

            foreach (Ingredient r in reader)
            {
                actual.Add(r);
            }

            var expectedJson = JsonSerializer.Serialize(expected);
            var actualJson = JsonSerializer.Serialize(actual);

            Assert.Equal(expectedJson, actualJson);
        }

        [Theory(DisplayName = "Reads File With UTF-8 Encoding")]
        [InlineData("index,input,name,qty,range_end,unit,comment\n" +
                    "1,2 jalape\xc3\xb1os,jalape\xc3\xb1os,2.0,0.0,,,")]
        public void TestReadsFileWithUtf8Encoding(string input)
        {
            var expected = new Ingredient
            {
                Input = "2 jalape\xc3\xb1os",
                Name = "jalape\xc3\xb1os",
                Quantity = 2.0f,
                Unit = "",
                RangeEnd = 0.0f,
                Comment = ""
            };

            var reader = new Reader(new StringReader(input));
            reader.MoveNext();

            var actual = (Ingredient)reader.Current;

            var expectedJson = JsonSerializer.Serialize(expected);
            var actualJson = JsonSerializer.Serialize(actual);

            Assert.Equal(expectedJson, actualJson);
        }

        [Theory(DisplayName = "Interprets Empty Range End as Zero")]
        [InlineData("index,input,name,qty,range_end,unit,comment\n" +
                    "77,3 bananas,bananas,3.0,,,")]
        public void TestInterpretsEmptyRangeEndAsZero(string input)
        {
            var expected = new Ingredient
            {
                Input = "3 bananas",
                Name = "bananas",
                Quantity = 3.0f,
                Unit = "",
                RangeEnd = 0.0f,
                Comment = ""
            };

            var reader = new Reader(new StringReader(input));
            reader.MoveNext();

            var actual = (Ingredient)reader.Current;

            var expectedJson = JsonSerializer.Serialize(expected);
            var actualJson = JsonSerializer.Serialize(actual);

            Assert.Equal(expectedJson, actualJson);
        }

        [Theory(DisplayName = "Raises error when CSV does not have required columns")]
        [InlineData("index,input,UNEXPECTED_COLUMN,qty,range_end,unit,comment\n" +
                    "77,3 bananas,bananas,3.0,,,")]
        public void TestRaisesErrorWhenCsvDoesNotHaveRequiredColumns(string input)
        {
            var reader = new Reader(new StringReader(input));

            Assert.Throws<CsvHelper.HeaderValidationException>(() => reader.MoveNext());
        }
    }

}