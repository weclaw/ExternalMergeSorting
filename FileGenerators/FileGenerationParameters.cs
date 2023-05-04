namespace LargeFileSorting.FileGenerators
{
    internal struct FileGenerationParameters
    {
        public FileGenerationParameters(string outputFilePath, 
            short lineLengthStringMin, 
            short lineLengthStringMax, 
            int targetFileSizeMBytes,
            int duplicatesRatio)
        {
            OutputFilePath = outputFilePath;
            LineLengthStringMin = lineLengthStringMin;
            LineLengthStringMax = lineLengthStringMax;
            TargetFileSizeMBytes = targetFileSizeMBytes;
            DuplicatesRatio = duplicatesRatio;
        }

        public string OutputFilePath { get; private set; }
        public int TargetFileSizeMBytes { get; private set; }
        public short LineLengthStringMin { get; private set; }
        public short LineLengthStringMax { get; private set; }
        public int DuplicatesRatio { get; private set; }
    }
}
