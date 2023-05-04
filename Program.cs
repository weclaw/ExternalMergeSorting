using CommandLine;
using FluentValidation.Results;
using LargeFileSorting.FileGenerators;
using LargeFileSorting.InputArguments;
using LargeFileSorting.Sorting;
using System.Diagnostics;
using static LargeFileSorting.InputArguments.LargeFileSortingToolArguments;

namespace LargeFileSorting
{
    internal class Program
    {
        static void Main()
        {
            LargeFileSortingToolArguments arguments = GetValidInputArguments();

            switch (arguments.Mode)
            {
                case ExecutionMode.GenerateFile:
                    ExecuteFileGeneration(arguments.GetFileGenerationParameters());
                    break;
                case ExecutionMode.SortFile:
                    ExecuteSort(arguments.GetSortingParameters());
                    break;
            };
        }

        private static LargeFileSortingToolArguments GetValidInputArguments()
        {
            LargeFileSortingToolArguments? arguments = null;
            ValidationResult validationResult = null;

            Console.WriteLine($"Please provide correct input arguments. For more info use --help.");

            do
            {
                var args = Console.ReadLine().SplitArgs();

                ParserResult<LargeFileSortingToolArguments> result = Parser.Default
                              .ParseArguments<LargeFileSortingToolArguments>(args)
                              .WithParsed(result => arguments = result);

                var validator = GetCorrectValidator(arguments);
                validationResult = validator?.ValidateArguments(arguments);

                if(!validationResult?.IsValid ?? true)
                {
                    arguments = null;
                    validationResult?.Errors.ForEach(e => Console.WriteLine(e.ErrorMessage));
                }
            }
            while (arguments == null);

            return arguments;
        }

        private static IArgumentsValidator<LargeFileSortingToolArguments> GetCorrectValidator(LargeFileSortingToolArguments? arguments)
        {
            switch(arguments?.Mode)
            {
                case ExecutionMode.GenerateFile:
                    return new FileGenerationArgumentsValidator();
                case ExecutionMode.SortFile:
                    return new SortingArgumentsValidator();
                default:
                    return null;
            }
        }

        private static void ExecuteSort(SortingParameters sortingParameters)
        {;
            var sorter = new ExternalMergeSorter(sortingParameters);
            sorter.Sort();
        }

        private static void ExecuteFileGeneration(FileGenerationParameters parameters)
        {
            var generator = new RandomStringFileGenerator(parameters);
            var duplicates = generator.Generate();

            Console.WriteLine($"Written {duplicates} duplicated rows.");
        }
    }
}