using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsharpIngredientPhraseTagger.Training
{
    public class Ingredient
    {
        [Name("input")]
        public string? Input { get; set; }

        [Name("name")]
        public string? Name { get; set; }

        [Name("qty")]
        public float? Quantity { get; set; }

        [Name("range_end")]
        public float? RangeEnd { get; set; }

        [Name("unit")]
        public string? Unit { get; set; }

        [Name("comment")]
        public string? Comment { get; set; }
    }
}
