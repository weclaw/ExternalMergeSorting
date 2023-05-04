using System.Data;
using System.Text;

namespace LargeFileSorting.Sorting
{
    internal class ExternalMergeSorter : IFileSorter
    {
        private const short ULONG_MAX_LENGTH = 20;
        private readonly string _inputFilePath;
        private readonly int _fileBufferSizeBytes = 50000;
        private readonly string _inputFileDirPath;
        private readonly int _splitChunkSize;
        private readonly bool _skipCleanup;

        private readonly IFileMerger _merger;

        public ExternalMergeSorter(SortingParameters sortingParameters)
        {
            if (!File.Exists(sortingParameters.InputFilePath))
                throw new ArgumentException("Provided file does not exist!");

            _inputFilePath = sortingParameters.InputFilePath;
            _inputFileDirPath = Directory.GetParent(_inputFilePath).FullName;
            _splitChunkSize = sortingParameters.ChunkLinesLimit;
            _skipCleanup = sortingParameters.SkipCleanup;

            _merger = new KWayMerger(_inputFileDirPath, sortingParameters.SkipCleanup, sortingParameters.MergeKParameter);
        }

        public void Sort()
        {
            var splittedFiles = Split();

            var finalFile = _merger.Merge(splittedFiles, true);

            if (!_skipCleanup)
                Directory.Delete($"{_inputFileDirPath}\\temp");
            
        }

        private List<string> Split()
        {
            using (FileStream fs = new FileStream(_inputFilePath, FileMode.Open, FileAccess.Read, FileShare.None, _fileBufferSizeBytes, FileOptions.SequentialScan))
            using (BufferedStream bs = new BufferedStream(fs, _fileBufferSizeBytes))
            using (StreamReader sr = new StreamReader(bs))
            {
                string outputFilePathBase = $"{_inputFileDirPath}\\temp";
                int iteration = 0;
                List<Task<string>> sortAndSaveTasks = new List<Task<string>>();

                Directory.CreateDirectory(outputFilePathBase);

                while (!sr.EndOfStream)
                {
                    int i = 0;
                    string[] array = new string[_splitChunkSize];

                    do
                    {
                        array[i++] = TransormRecord(sr.ReadLine());
                    }
                    while (!sr.EndOfStream && i < _splitChunkSize);

                    if (sr.EndOfStream)
                    {
                        var newArray = new string[i];
                        Array.Copy(array, 0, newArray, 0, i);
                        array = newArray;
                    }

                    var sortInputParams = new SortAndSaveInputParameters(outputFilePathBase, iteration++, array);
                    sortAndSaveTasks.Add(Task<string>.Factory.StartNew(() => SortAndSaveArray(sortInputParams) ));
                }

                Task.WaitAll(sortAndSaveTasks.Where(t => !t.IsCompleted).ToArray());

                return sortAndSaveTasks.Select(t => t.Result).ToList();
            }
        }

        private string TransormRecord(string line)
        {
            var splitedArray = line.Split('.', 2);
            return $"{splitedArray[1]}.{splitedArray[0].PadLeft(ULONG_MAX_LENGTH, '-')}";
        }

        private string SortAndSaveArray(SortAndSaveInputParameters input)
        {
            var fileName = $"{input.OutputFilePathBase}\\{input.Iteration}";

            Array.Sort(input.InputArray, StringComparer.Ordinal);
            File.WriteAllLines(fileName, input.InputArray);

            return fileName;
        }

        internal struct SortAndSaveInputParameters
        {
            public string OutputFilePathBase { get; init; }
            public int Iteration { get; init; }
            public string[] InputArray { get; init; }

            public SortAndSaveInputParameters(string outputFilePathBase, int iteration, string[] array)
            {
                this.OutputFilePathBase = outputFilePathBase;
                this.Iteration = iteration;
                InputArray = array;
            }
        }
    }
}