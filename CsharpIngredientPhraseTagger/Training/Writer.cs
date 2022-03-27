using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsharpIngredientPhraseTagger.Training
{
    /// <summary>
    /// Writes labelled ingredient data to a CSV file.
    /// </summary>
    public class Writer : IDisposable
    {
        private readonly CsvWriter csvWriter;
        private readonly TextWriter textWriter;

        public Writer(TextWriter textWriter)
        {
            this.textWriter = textWriter;
            csvWriter = new CsvWriter(this.textWriter, CultureInfo.InvariantCulture);
            csvWriter.WriteHeader<Ingredient>();
            csvWriter.NextRecord();
        }

        public void Dispose()
        {
            csvWriter.Dispose();
        }

        /// <summary>
        /// Adds a row of data to the output CSV file.
        /// Args:
        ///     row: A dictionary of values for a labelled ingredient.The
        ///         dictionary must contain the following keys:
        ///         * input
        ///         * name
        ///         * qty
        ///         * range_end
        ///         * unit
        ///         * comment
        /// </summary>
        /// <param name="row">A dictionary of values for a labeled ingredient</param>
        public void WriteRow(Ingredient row)
        {
            csvWriter.WriteRecord(row);
        }

        /// <summary>
        /// Writes multiple rows to the output CSV file.
        /// </summary>
        /// <param name="rows">A list of values for labeled ingredients</param>
        public void WriteRows(List<Ingredient> rows)
        {
            csvWriter.WriteRecords(rows);
        }
    }
}
