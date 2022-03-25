using CsharpIngredientPhraseTagger.Training;

var reader = new Reader("nyt-ingredients-snapshot-2015.csv");

while(reader.MoveNext())
{
    var current = (Dictionary<string, string>)reader.Current;
    Console.WriteLine(Translator.TranslateRow(current));
}
