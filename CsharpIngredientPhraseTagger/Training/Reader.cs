using CsvHelper;
using CsvHelper.Configuration;
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
    public class Reader : IEnumerable, IEnumerator, IDisposable
    {
        private TextReader reader;
        private CsvReader csvReader;
        private readonly List<Ingredient> ingredients = new();

        public Reader(TextReader textReader)
        {
            reader = textReader;
            Reset();
        }

        public object Current
        {
            get
            {
                return ingredients.LastOrDefault();
            }
        }

        public void Dispose()
        {
            csvReader.Dispose();
            ingredients.Clear();
        }

        public IEnumerator GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            var isRead = csvReader.Read();
            if (!ingredients.Any())
            {
                csvReader.Read();
                csvReader.ReadHeader();
                csvReader.ValidateHeader<Ingredient>();
            }
            var record = csvReader.GetRecord<Ingredient>();
            record.Quantity = record.Quantity == null ? 0.0f : record.Quantity;
            record.RangeEnd = record.RangeEnd == null ? 0.0f : record.RangeEnd;
            ingredients.Add(record);
            return isRead;
        }

        public void Reset()
        {
            ingredients.Clear();
            csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim
            });
        }
    }
}
