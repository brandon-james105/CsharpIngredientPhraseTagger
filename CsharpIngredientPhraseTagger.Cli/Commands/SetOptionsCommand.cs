using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsharpIngredientPhraseTagger.Exec.Commands
{
    public interface ICommand
    {
        void Execute();
    }

    public class SetOptionsCommand : ICommand
    {
        [Option("model-dir", HelpText = "Set the directory of the models")]
        public string ModelDir { get; set; }

        [Option("model-file", HelpText = "Set the model file name")]
        public string ModelFile { get; set; }

        [Option("data-path", HelpText = "Set the data path")]
        public string DataPath { get; set; }

        public void Execute()
        {
            Environment.SetEnvironmentVariable("MODEL_DIR", ModelDir);
            Environment.SetEnvironmentVariable("MODEL_FILE", ModelFile);
            Environment.SetEnvironmentVariable("LABELLED_DATA_FILE", DataPath);
        }
    }
}
