using CommandLine;
using CommandLine.Text;

namespace LargeFileSorting.InputArguments
{
    internal struct LargeFileSortingToolArguments
    {
        [Option('m', "executionMode", HelpText = "Execution mode for our tool. GenerateFile=0, SortFile=1")]
        public ExecutionMode Mode { get; set; }

        [Option('p', "filePath", HelpText = "Path to a file")]
        public string? FilePath { get; set; }

        [Option('q', "quit", HelpText = "Giving up? Quit application")]
        public bool QuitCommand { get; set; }

        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Generate new file and save at a given path", new LargeFileSortingToolArguments { Mode = ExecutionMode.GenerateFile, FilePath = "input/file.txt" });
                yield return new Example("Process file from given path using custom implementation of External Merge Sort with K-Way Merge", new LargeFileSortingToolArguments { Mode = ExecutionMode.SortFile, FilePath = "input/file.txt" });
            }
        }

        public enum ExecutionMode
        {
            GenerateFile,
            SortFile
        }
    }
}
