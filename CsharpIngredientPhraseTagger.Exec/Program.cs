using CommandLine;
using CRFSharpWrapper;
using CsharpIngredientPhraseTagger.Exec;
using CsharpIngredientPhraseTagger.Exec.Commands;
using CsharpIngredientPhraseTagger.Training;

// Train

// Partition Labels

Parser.Default.ParseArguments<SetOptionsCommand>(args)
              .WithParsed(t => t.Execute());

var outputDir = args.ElementAtOrDefault(1) != null ? args.ElementAtOrDefault(1) : "output";
Environment.SetEnvironmentVariable("OUTPUT_DIR", outputDir);

var labelledDataFile = Environment.GetEnvironmentVariable("LABELLED_DATA_FILE");
var trainingDataPercent = 0.9;
var labelledExampleCount = 1000;
var crfTrainingThreads = 2;

if (int.TryParse(Environment.GetEnvironmentVariable("LABELLED_EXAMPLE_COUNT"), out int parsedLabelledExampleCount))
{
    labelledExampleCount = parsedLabelledExampleCount;
}
if (double.TryParse(Environment.GetEnvironmentVariable("TRAINING_DATA_PERCENT"), out double parsedTrainingDataPercent))
{
    trainingDataPercent = parsedTrainingDataPercent;
}
if (int.TryParse(Environment.GetEnvironmentVariable("CRF_TRAINING_THREADS"), out int parsedCrfTrainingThreads))
{
    crfTrainingThreads = parsedCrfTrainingThreads;
}

Environment.SetEnvironmentVariable("LABELLED_EXAMPLE_COUNT", labelledExampleCount.ToString());
Environment.SetEnvironmentVariable("TRAINING_DATA_PERCENT", trainingDataPercent.ToString());
Environment.SetEnvironmentVariable("CRF_TRAINING_THREADS", crfTrainingThreads.ToString());

var countTrain = trainingDataPercent * labelledExampleCount;
var countTest = (1.0 - trainingDataPercent) * labelledExampleCount;

var trainingLabelsFile = $"{outputDir}/training_labels.csv";
var testingLabelsFile = $"{outputDir}/testing_labels.csv";

var crfTrainingFile = $"{outputDir}/training_data.crf";
var crfTestingFile = $"{outputDir}/testing_data.crf";

var crfLearnTemplate = "template_file";

var crfModelFile = $"{outputDir}/{DateTime.Now:yyyy-mm-dd_HM}-{labelledDataFile.Replace(".csv", "")}.crfmodel";

var testingOutputFile = $"{outputDir}/testing_output";
var evalOutputFIle = $"{outputDir}/eval_output";

var labelReader = new Reader(File.OpenText(labelledDataFile));
var trainingWriter = new Writer(File.CreateText(trainingLabelsFile));
var testingWriter = new Writer(File.CreateText(testingLabelsFile));

// Partition labels

Partitioner.SplitLabels(labelReader, trainingWriter, testingWriter, trainingDataPercent, labelledExampleCount);

// Generate data

using (var trainingReader = new Reader(File.OpenText(trainingLabelsFile)))
{
    var i = 0;
    while (trainingReader.MoveNext() && (labelledExampleCount != 0 ? i++ < labelledExampleCount : true))
    {
        var current = (Ingredient)trainingReader.Current;
        var translatedRow = Translator.TranslateRow(current);
        File.AppendAllText(crfTrainingFile, translatedRow + "\n");
        Console.WriteLine(translatedRow);
    }
}

using (var testingReader = new Reader(File.OpenText(testingLabelsFile)))
{
    var i = 0;
    while (testingReader.MoveNext() && (labelledExampleCount != 0 ? i++ < labelledExampleCount : true))
    {
        var current = (Ingredient)testingReader.Current;
        var translatedRow = Translator.TranslateRow(current);
        File.AppendAllText(crfTestingFile, translatedRow + "\n");
        Console.WriteLine(translatedRow);
    }
}

// Learn

new Encoder().Learn(new EncoderArgs
{
    strTemplateFileName = crfLearnTemplate,
    strTrainingCorpus = crfTrainingFile,
    strEncodedModelFileName = crfModelFile,
    threads_num = crfTrainingThreads,
    C = trainingDataPercent
});

// Test

var crfWrapper = new Decoder();

var options = new DecoderArgs
{
    strModelFileName = crfModelFile,
    strInputFileName = crfTestingFile,
    strOutputFileName = testingOutputFile
};

// Load encoded model from file
crfWrapper.LoadModel(options.strModelFileName);

// Create decoder tagger instance. If the running environment is multi-threads, each thread needs a separated instance

var tagger = crfWrapper.CreateTagger(options.nBest, options.maxword);
tagger.set_vlevel(options.probLevel);

// Initialize result
var crf_out = new crf_seg_out[options.nBest];
for (var i = 0; i < options.nBest; i++)
{
    crf_out[i] = new crf_seg_out(tagger.crf_max_word_num);
}

// Process 
// Build feature set from given test text. (this text causes an error)
List<List<string>> featureSet = BuildFeatureSet("1 cup butter, softened");

crfWrapper.Segment(crf_out, tagger, featureSet);

// An example for feature set builidng. Only use 1-dim character based feature  
List<List<string>> BuildFeatureSet(string str)
{
    List<List<string>> sinbuf = new List<List<string>>();
    foreach (char ch in str)
    {
        sinbuf.Add(new List<string>());
        sinbuf[sinbuf.Count - 1].Add(ch.ToString());
    }
    return sinbuf;
}

// Evaluate

File.WriteAllText(evalOutputFIle, Evaluator.Evaluate(testingOutputFile));