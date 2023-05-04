using FluentValidation.Results;

namespace LargeFileSorting.InputArguments
{
    internal interface IArgumentsValidator<T>
    {
        public ValidationResult ValidateArguments(T args);
    }
}