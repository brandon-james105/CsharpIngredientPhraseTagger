using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsharpIngredientPhraseTagger.Training
{
    /// <summary>
    /// Splits a full label set into a training and testing set.
    ///        Given a full set of labels and associated inputs, splits up the labels
    ///    into training and testing sets in the proportion defined by
    ///    training_fraction.
    ///    Args:
    ///        label_reader: A labelled_data.Reader instance that reads the full set
    ///            of labels.
    ///        training_label_writer: A labelled_data.Writer instance that writes out
    ///            the subset of labels to be used for training.
    ///        testing_label_writer: A labelled_data.Writer instance that writes out
    ///            the subset of labels to be used for testing.
    ///        training_fraction: A value between 0.0 and 1.0 that specifies the
    ///            proportion of labels to use for training.Any label not used for
    ///            training is used for testing until max_labels is reached.
    ///        max_labels: The maximum number of labels to read from label_reader. 0
    ///            is treated as infinite.
    /// </summary>
    public class Partitioner
    {
        public static void SplitLabels(Reader labelReader,
                                       Writer trainingLabelWriter,
                                       Writer testingLabelWriter,
                                       decimal trainingFraction,
                                       int maxLabels = 0)
        {
            var labels = ReadLabels(labelReader, maxLabels);
            WriteLabels(labels, trainingLabelWriter, testingLabelWriter, trainingFraction);
        }

        public static List<Dictionary<string, string>> ReadLabels(Reader reader, int maxLabels)
        {
            var labels = new List<Dictionary<string, string>>();
            var i = 0;

            foreach(Dictionary<string, string> item in reader)
            {
                if (maxLabels != 0 && i >= maxLabels)
                {
                    break;
                }
                labels.Add(item);
                i++;
            }
            return labels;
        }

        public static void WriteLabels(List<Dictionary<string, string>> labels, Writer trainingLabelWriter, Writer testingLabelWriter, decimal trainingFraction)
        {
            var trainingLabelCount = Convert.ToInt32(labels.Count() * trainingFraction);
            trainingLabelWriter.WriteRows(labels.Take(trainingLabelCount).ToList());
            testingLabelWriter.WriteRows(labels.Take(labels.Count - trainingLabelCount).ToList());
        }
    }
}
