namespace LargeFileSorting.InputArguments
{
    internal interface IArgumentsValidator<T>
    {
        public bool IsValid(T args);
    }
}