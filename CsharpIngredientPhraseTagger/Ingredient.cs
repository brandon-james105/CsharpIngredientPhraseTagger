using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsharpIngredientPhraseTagger
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

    public class IngredientMap : ClassMap<Ingredient>
    {
        public IngredientMap()
        {
            Map(m => m.Input).Name("input");
            Map(m => m.Name).Name("name");
            Map(m => m.Quantity).Name("qty")
                .TypeConverterOption.NullValues("0.0")
                .TypeConverterOption.Format("#0.0#");
            Map(m => m.RangeEnd).Name("range_end")
                .TypeConverterOption.NullValues("0.0")
                .TypeConverterOption.Format("#0.0#");
            Map(m => m.Unit).Name("unit");
            Map(m => m.Comment).Name("comment");
        }
    }

}
