using FluentValidation;
using FluentValidation.Results;
using System;

namespace LargeFileSorting.InputArguments
{
    internal class SortingArgumentsValidator : AbstractValidator<LargeFileSortingToolArguments>, IArgumentsValidator<LargeFileSortingToolArguments>
    {
        public SortingArgumentsValidator()
        {
            RuleFor(x => x.Mode)
                .NotNull()
                .WithMessage("Please specify execution mode. Option: -m");

            When(x => x.Mode.HasValue,
                 () => RuleFor(x => x.Mode).Must(x => Enum.IsDefined(typeof(LargeFileSortingToolArguments.ExecutionMode), x))
                .WithMessage("Incorrect value for Execution Mode. Allowed Values 0 and 1"));

            RuleFor(x => x.FilePath)
                .NotEmpty()
                .WithMessage("Please specify a file path. Option -p");

            When(x => !string.IsNullOrEmpty(x.FilePath), 
                () => RuleFor(x => x.FilePath).Must(x => File.Exists(x))
                      .WithMessage("Please specify a file path to an existing file"));
        }

        public ValidationResult ValidateArguments(LargeFileSortingToolArguments args)
        {
            if(args == null)
            {
                var result = new ValidationResult();
                result.Errors.Add(new ValidationFailure("InputArguments", "Couldn't parse the input parameters"));
                return result;
            }
            
            return this.Validate(args);
        }
    }
}
