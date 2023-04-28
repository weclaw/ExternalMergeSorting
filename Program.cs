using CommandLine;
using LargeFileSorting.InputArguments;
using LargeFileSorting.Sorting;
using System.Diagnostics;
using static LargeFileSorting.InputArguments.LargeFileSortingToolArguments;

namespace LargeFileSorting
{
    internal class Program
    {
        //In larger setup of course should be passed in through DI
        private static readonly IArgumentsValidator<LargeFileSortingToolArguments>? _validator = new LargeFileSortingArgumentsValidator();

        static void Main()
        {
            var sw = new Stopwatch();
            sw.Start();
            LargeFileSortingToolArguments arguments = new LargeFileSortingToolArguments { FilePath = "C:\\Users\\WeclawskiAndrzej\\source\\repos\\ExternalMergeSort\\inputMIT20GB", Mode = ExecutionMode.SortFile }; //GetValidInputArguments();

            if (arguments.QuitCommand)
                return;

            switch(arguments.Mode)
            {
                case ExecutionMode.GenerateFile:
                    ExecuteFileGeneration(arguments.FilePath);
                    break;
                case ExecutionMode.SortFile:
                    ExecuteSort(arguments.FilePath);
                    break;
            };

            sw.Stop();
            Console.WriteLine($"{sw.Elapsed.TotalSeconds}");
        }

        private static LargeFileSortingToolArguments GetValidInputArguments()
        {
            LargeFileSortingToolArguments? arguments = null;

            do
            {
                Console.WriteLine($"Please provide correct input arguments. For more info use --help.");
                var args = Console.ReadLine().SplitArgs();

                ParserResult<LargeFileSortingToolArguments> result = Parser.Default
                              .ParseArguments<LargeFileSortingToolArguments>(args)
                              .WithParsed(result => arguments = result);
            }
            while (arguments == null || !_validator.IsValid(arguments.Value));

            return arguments.Value;
        }

        private static void ExecuteSort(string filePath)
        {;
            var sorter = new ExternalMergeSorter(new SortingParameters(filePath, 10000, 200));
            sorter.Sort();
        }

        private static void ExecuteFileGeneration(string filePath)
        {
            var sorter = new ExternalMergeSorter(new SortingParameters(filePath, 10000, 200));
            sorter.Sort();
        }
    }
}