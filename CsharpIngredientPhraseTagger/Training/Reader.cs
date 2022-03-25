using CsvHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsharpIngredientPhraseTagger.Training
{
    public class Reader : IEnumerable, IEnumerator
    {
        private string fileName;
        private CsvReader csvReader;
        List<Dictionary<string, string>> ingredients = new();

        public Reader(string fileName)
        {
            this.fileName = fileName;
            csvReader = new CsvReader(File.OpenText(fileName), CultureInfo.InvariantCulture);
            csvReader.Read();
            csvReader.ReadHeader();
            csvReader.ValidateHeader<Ingredient>();
        }

        public object Current
        {
            get
            {
                var record = ReadRow(csvReader.Context.Parser.RawRecord);
                ingredients.Add(record);
                return ingredients.Last();
            }
        }

        public IEnumerator GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            return csvReader.Read();
        }

        public Dictionary<string, string> ReadRow(string row)
        {
            var validFields = new HashSet<string>() { "input", "name", "qty", "range_end", "unit", "comment" };
            var rowValues = new Dictionary<string, string>();
            var stringReader = new StringReader(row);
            var reader = new CsvReader(stringReader, CultureInfo.InvariantCulture);
            reader.Read();

            foreach (var field in validFields)
            {
                rowValues.Add(field, reader.GetField(csvReader.GetFieldIndex(field)));
            }

            return rowValues;
        }

        public void Reset()
        {
            ingredients.Clear();
            csvReader.Dispose();
            csvReader = new CsvReader(File.OpenText(fileName), CultureInfo.InvariantCulture);
            csvReader.Read();
            csvReader.ReadHeader();
            csvReader.ValidateHeader<Ingredient>();
        }
    }
}
