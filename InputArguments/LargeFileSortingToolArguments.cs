using CommandLine;
using CommandLine.Text;
using LargeFileSorting.FileGenerators;
using LargeFileSorting.Sorting;

namespace LargeFileSorting.InputArguments
{
    internal class LargeFileSortingToolArguments
    {
        [Option('m', "executionMode", Required = true, HelpText = "Execution mode for our tool. GenerateFile=0, SortFile=1")]
        public ExecutionMode? Mode { get; set; }

        [Option('p', "filePath", Required = true, HelpText = "Path to a file")]
        public string? FilePath { get; set; }

        //Generation Specific
        [Option('s', "TargetFileSize", HelpText = "Size of a file in MBytes to be generated.")]
        public int? TargetFileSize { get; set; }

        [Option('l', "LineLengthStringMin", Default = (short)50, HelpText = "Minimum length of a generated Text part of a file record.")]
        public short LineLengthStringMin { get; set; }

        [Option('e', "LineLengthStringMax", Default = (short)500, HelpText = "Maximum length of a generated Text part of a file record.")]
        public short LineLengthStringMax { get; set; }

        [Option('r', "DuplicatesRatio", Default = (short)1000, HelpText = "Ratio of records with duplicated text parts. Statistically every X record to have a duplicate.")]
        public short DuplicatesRatio { get; set; }


        //Sorting Specific
        [Option('c', "ChunkSizeLines", Default = 100000, HelpText = "Control over how program is going to split files for sorting hence how will utilize memory. Amount of lines per file.")]
        public int ChunkSizeLines { get; set; }

        [Option('d', "SkipCleanup", Default = false, HelpText = "Skip deleting temporary files. Turn it on only for debugging and program execution analysis purposes.")]
        public bool SkipCleanup { get; set; }

        [Option('k', "MergeKParameter", Default = (short)8, HelpText = "Optimization parameter for K-way merge. Max amount of files for one run of merge.")]
        public short MergeKParameter { get; set; }

        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Generate new file and save at a given path", 
                    new LargeFileSortingToolArguments { Mode = ExecutionMode.GenerateFile, FilePath = "input/file.txt" });
                yield return new Example("Process file from given path using custom implementation of External Merge Sort with K-Way Merge", 
                    new LargeFileSortingToolArguments { Mode = ExecutionMode.SortFile, FilePath = "input/file.txt" });
            }
        }

        public enum ExecutionMode
        {
            GenerateFile,
            SortFile
        }

        internal FileGenerationParameters GetFileGenerationParameters()
        {
            return new FileGenerationParameters(FilePath, LineLengthStringMin, LineLengthStringMax, TargetFileSize.Value, DuplicatesRatio);
        }

        internal SortingParameters GetSortingParameters()
        {
            return new SortingParameters(FilePath, ChunkSizeLines, SkipCleanup, MergeKParameter);
        }
    }
}
