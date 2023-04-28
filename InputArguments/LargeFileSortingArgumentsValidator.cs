namespace LargeFileSorting.InputArguments
{
    internal struct LargeFileSortingArgumentsValidator : IArgumentsValidator<LargeFileSortingToolArguments>
    {
        public bool IsValid(LargeFileSortingToolArguments args)
        {
            return args switch
            {
                { QuitCommand: true } => true,
                { FilePath: null } => false,
                _ => true
            };
        }
    }
}
