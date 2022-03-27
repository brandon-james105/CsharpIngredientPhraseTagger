using CsharpIngredientPhraseTagger.Training;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CsharpIngredientPhraseTagger.Tests
{
    public class TestPartitioner
    {
        private MemoryStream mockTrainingFile;
        private MemoryStream mockTestingFile;
        private Writer mockTrainingFileWriter;
        private Writer mockTestingFileWriter;

        public TestPartitioner()
        {
            mockTrainingFile = new MemoryStream();
            TextWriter trainingFileTextWriter = new StreamWriter(mockTrainingFile);
            mockTestingFile = new MemoryStream();
            TextWriter testingFileTextWriter = new StreamWriter(mockTestingFile);

            mockTrainingFileWriter = new Writer(trainingFileTextWriter);
            mockTestingFileWriter = new Writer(testingFileTextWriter);
        }
        
        [Fact(DisplayName = "Partition 80% training")]
        public void TestPartition80PercentTraining()
        {
            var mockLabelReader = new Reader(new StringReader(
                                             "input,name,qty,range_end,unit,comment\n" +
                                             "1 cup foo,foo,1.0,0.0,cup,\n" +
                                             "2 drops foz,foz,2.0,0.0,drop,\n" +
                                             "3 ml faa,faa,3.0,0.0,ml,\n" +
                                             "4 cloves bar,bar,4.0,0.0,cloves,\n" +
                                             "5 oz baz,baz,5.0,0.0,oz,"));
            Partitioner.SplitLabels(mockLabelReader,
                                    mockTrainingFileWriter,
                                    mockTestingFileWriter,
                                    0.8);

            var mockTrainingFileExpected = "input,name,qty,range_end,unit,comment\n" +
                                           "1 cup foo,foo,1.0,0.0,cup,\n" +
                                           "2 drops foz,foz,2.0,0.0,drop,\n" +
                                           "3 ml faa,faa,3.0,0.0,ml,\n" +
                                           "4 cloves bar,bar,4.0,0.0,cloves,";
            var mockTrainingFileActual = Encoding.UTF8.GetString(mockTrainingFile.ToArray()).Trim();

            Assert.Equal(mockTrainingFileExpected, mockTrainingFileActual);

            var mockTestingFileExpected = "input,name,qty,range_end,unit,comment\n" +
                                          "5 oz baz,baz,5.0,0.0,oz,";
            var mockTestingFileActual = Encoding.UTF8.GetString(mockTestingFile.ToArray()).Trim();

            Assert.Equal(mockTestingFileExpected, mockTestingFileActual);
        }

        [Fact(DisplayName = "Partition 20% training")]
        public void TestPartition20PercentTraining()
        {
            var mockLabelReader = new Reader(new StringReader(
                                             "input,name,qty,range_end,unit,comment\n" +
                                             "1 cup foo,foo,1.0,0.0,cup,\n" +
                                             "2 drops foz,foz,2.0,0.0,drop,\n" +
                                             "3 ml faa,faa,3.0,0.0,ml,\n" +
                                             "4 cloves bar,bar,4.0,0.0,cloves,\n" +
                                             "5 oz baz,baz,5.0,0.0,oz,"));
            Partitioner.SplitLabels(mockLabelReader,
                                    mockTrainingFileWriter,
                                    mockTestingFileWriter,
                                    0.2);

            var mockTrainingFileExpected = "input,name,qty,range_end,unit,comment\n" +
                                           "1 cup foo,foo,1.0,0.0,cup,";
            var mockTrainingFileActual = Encoding.UTF8.GetString(mockTrainingFile.ToArray()).Trim();

            Assert.Equal(mockTrainingFileExpected, mockTrainingFileActual);

            var mockTestingFileExpected = "input,name,qty,range_end,unit,comment\n" +
                                          "2 drops foz,foz,2.0,0.0,drop,\n" +
                                          "3 ml faa,faa,3.0,0.0,ml,\n" +
                                          "4 cloves bar,bar,4.0,0.0,cloves,\n" +
                                          "5 oz baz,baz,5.0,0.0,oz,";
            var mockTestingFileActual = Encoding.UTF8.GetString(mockTestingFile.ToArray()).Trim();

            Assert.Equal(mockTestingFileExpected, mockTestingFileActual);
        }

        [Fact(DisplayName = "Partition with max labels discards labels")]
        public void TestPartitionWithMaxLabelsDiscardsLabels()
        {
            var mockLabelReader = new Reader(new StringReader(
                                                "input,name,qty,range_end,unit,comment\n" +
                                                "1 cup foo,foo,1.0,0.0,cup,\n" +
                                                "2 drops foz,foz,2.0,0.0,drop,\n" +
                                                "3 ml faa,faa,3.0,0.0,ml,\n" +
                                                "4 cloves bar,bar,4.0,0.0,cloves,\n" +
                                                "5 oz baz,baz,5.0,0.0,oz,"));
            Partitioner.SplitLabels(mockLabelReader,
                                    mockTrainingFileWriter,
                                    mockTestingFileWriter,
                                    0.67,
                                    3);

            var mockTrainingFileExpected = "input,name,qty,range_end,unit,comment\n" +
                                           "1 cup foo,foo,1.0,0.0,cup,\n" +
                                           "2 drops foz,foz,2.0,0.0,drop,";
            var mockTrainingFileActual = Encoding.UTF8.GetString(mockTrainingFile.ToArray()).Trim();

            Assert.Equal(mockTrainingFileExpected, mockTrainingFileActual);

            var mockTestingFileExpected = "input,name,qty,range_end,unit,comment\n" +
                                          "3 ml faa,faa,3.0,0.0,ml,";
            var mockTestingFileActual = Encoding.UTF8.GetString(mockTestingFile.ToArray()).Trim();

            Assert.Equal(mockTestingFileExpected, mockTestingFileActual);
        }
    }
}
