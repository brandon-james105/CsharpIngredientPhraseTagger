using CsharpIngredientPhraseTagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CsharpIngredientPhraseTagger.Tests
{
    public class TestWriter
    {
        [Fact(DisplayName = "Writes valid rows")]
        public void TestWritesValidRows()
        {
            var store = new MemoryStream();

            TextWriter textWriter = new StreamWriter(store);
            var writer = new Writer(textWriter);
            writer.WriteRows(new List<Ingredient>()
            {
                new()
                {
                    Input = "4 to 6 large cloves garlic",
                    Quantity = 4,
                    Unit = "clove",
                    Name = "garlic",
                    RangeEnd = 6,
                    Comment = "",
                },
                new()
                {
                    Input = "3 bananas",
                    Quantity = 3,
                    Unit = "",
                    Name = "bananas",
                    Comment = "",
                    RangeEnd = 0,
                },
                new()
                {
                    Input = "2 1/2 pounds bell peppers (about 6 peppers in assorted colors), cut into 2-inch chunks",
                    Quantity = 2.5f,
                    Unit = "pound",
                    Name = "bell peppers",
                    RangeEnd = 0,
                    Comment = "(about 6 peppers in assorted colors), cut into 2-inch chunks",
                }
            });

            var expected = "input,name,qty,range_end,unit,comment\n" +
                           "4 to 6 large cloves garlic,garlic,4.0,6.0,clove,\n" +
                           "3 bananas,bananas,3.0,0.0,,\n" +
                           "\"2 1/2 pounds bell peppers (about 6 peppers in assorted colors), cut into 2-inch chunks\",bell peppers,2.5,0.0,pound,\"(about 6 peppers in assorted colors), cut into 2-inch chunks\"\n";
            var actual = Encoding.UTF8.GetString(store.ToArray());

            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "Writes valid rows one by one")]
        public void TestWritesRowsOneByOne()
        {
            var store = new MemoryStream();

            TextWriter textWriter = new StreamWriter(store);
            var writer = new Writer(textWriter);
            writer.WriteRow(
                new Ingredient()
                {
                    Input = "4 to 6 large cloves garlic",
                    Quantity = 4,
                    Unit = "clove",
                    Name = "garlic",
                    RangeEnd = 6,
                    Comment = "",
                });
            writer.WriteRow(
                new Ingredient()
                {
                    Input = "3 bananas",
                    Quantity = 3,
                    Unit = "",
                    Name = "bananas",
                    Comment = "",
                    RangeEnd = 0,
                });
            writer.WriteRow(
                new Ingredient()
                {
                    Input = "2 1/2 pounds bell peppers (about 6 peppers in assorted colors), cut into 2-inch chunks",
                    Quantity = 2.5f,
                    Unit = "pound",
                    Name = "bell peppers",
                    RangeEnd = 0,
                    Comment = "(about 6 peppers in assorted colors), cut into 2-inch chunks",
                });

            var expected = "input,name,qty,range_end,unit,comment\n" +
                           "4 to 6 large cloves garlic,garlic,4.0,6.0,clove,\n" +
                           "3 bananas,bananas,3.0,0.0,,\n" +
                           "\"2 1/2 pounds bell peppers (about 6 peppers in assorted colors), cut into 2-inch chunks\",bell peppers,2.5,0.0,pound,\"(about 6 peppers in assorted colors), cut into 2-inch chunks\"\n";
            var actual = Encoding.UTF8.GetString(store.ToArray());

            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "Writes with UTF-8 encoding")]
        public void TestWritesWithUtf8Encoding()
        {
            var store = new MemoryStream();

            TextWriter textWriter = new StreamWriter(store);
            var writer = new Writer(textWriter);

            writer.WriteRow(new Ingredient
            {
                Input = "2 jalape\xc3\xb1os",
                Name = "jalape\xc3\xb1os",
                Quantity = 2.0f,
                Unit = "",
                RangeEnd = 0.0f,
                Comment = ""
            });

            var expected = "input,name,qty,range_end,unit,comment\n2 jalape\xc3\xb1os,jalape\xc3\xb1os,2.0,0.0,,\n";
            var actual = Encoding.UTF8.GetString(store.ToArray());

            Assert.Equal(expected, actual);
        }
    }
}
