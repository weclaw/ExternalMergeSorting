using LargeFileSorting.Sorting;
using System.Security.Cryptography;

namespace LargeFileSorting.FileGenerators
{
    internal class RandomStringFileGenerator : IFileGenerator
    {
        private const long BYTES_IN_MG = 1024 * 1024;
        private const int FILE_BUFFER_SIZE = 50000;

        private readonly int _targetMBytes, _duplicatesRatio;
        private readonly short _lineLengthStringMin, _lineLengthStringMax;
        private readonly string _outputFilePath;
        private readonly char[] _availableChars;
        private readonly Queue<string> _duplicatesQueue = new Queue<string>();

        public RandomStringFileGenerator(FileGenerationParameters fileGenerationParameters)
        {
            _lineLengthStringMin = fileGenerationParameters.LineLengthStringMin;
            _lineLengthStringMax = fileGenerationParameters.LineLengthStringMax;
            _outputFilePath = fileGenerationParameters.OutputFilePath;
            _targetMBytes = fileGenerationParameters.TargetFileSizeMBytes;
            _duplicatesRatio = fileGenerationParameters.DuplicatesRatio;

            _availableChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ ".ToCharArray();
        }

        public int Generate()
        {
            using (FileStream fs = new FileStream(_outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None, FILE_BUFFER_SIZE))
            using (BufferedStream bs = new BufferedStream(fs, FILE_BUFFER_SIZE))
            using (StreamWriter outputFileStream = new StreamWriter(bs))
            {
                long writtenBytes = 0;
                int writtenDuplicates = 0;

                while (writtenBytes < _targetMBytes * BYTES_IN_MG)
                {
                    var text = GenerateTextPartWithDuplicates(writtenBytes, ref writtenDuplicates);
                    var longNumber = BitConverter.ToUInt64(RandomNumberGenerator.GetBytes(8), 0);
                    var line = $"{longNumber}. {text}";

                    outputFileStream.WriteLine(line);
                    writtenBytes += line.Length + 1;
                }

                return writtenDuplicates;
            }
        }

        private string GenerateTextPartWithDuplicates(long writtenBytes, ref int writtenDuplicates)
        {
            var numberOfChars = RandomNumberGenerator.GetInt32(_lineLengthStringMin, _lineLengthStringMax + 1);
            var newText = new string(RandomNumberGenerator.GetBytes(numberOfChars).Select(b => _availableChars[b % _availableChars.Length]).ToArray()).Trim();

            var duplicatesDrivingRandomNumber = RandomNumberGenerator.GetInt32(0, _duplicatesRatio + 1);

            if (duplicatesDrivingRandomNumber == 0 || SafeguardAgainstQuarterFileWithoutDuplicates(writtenBytes, writtenDuplicates))
            {
                _duplicatesQueue.Enqueue(newText); 
            }
            else if((duplicatesDrivingRandomNumber == _duplicatesRatio && _duplicatesQueue.Any()) 
                || SafeguardAgainstThirdQuarterFileWithoutDuplicates(writtenBytes, writtenDuplicates))
            {
                newText = _duplicatesQueue.Dequeue();
                writtenDuplicates++;
            }
            else if(duplicatesDrivingRandomNumber == _duplicatesRatio / 2 && _duplicatesQueue.Any())
            {
                newText = _duplicatesQueue.Peek();
                writtenDuplicates++;
            }

            return newText;
        }
        bool SafeguardAgainstQuarterFileWithoutDuplicates(long writtenBytes, int writtenDuplicates)
        {
            return writtenBytes > 0.25 * _targetMBytes * BYTES_IN_MG
                && writtenDuplicates == 0
                && _duplicatesQueue.Count() == 0;
        }

        bool SafeguardAgainstThirdQuarterFileWithoutDuplicates(long writtenBytes, int writtenDuplicates)
        {
            return writtenBytes > 0.75 * _targetMBytes * BYTES_IN_MG
                && writtenDuplicates == 0;
        }
    }
}
