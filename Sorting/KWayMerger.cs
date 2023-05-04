using System;

namespace LargeFileSorting.Sorting
{
    internal class KWayMerger : IFileMerger
    {
        private readonly bool _skipCleanup;
        private readonly string _inputFileDirPath;
        private readonly int _fileBufferSizeBytes = 50000;
        private readonly short _mergeKParameter;

        public KWayMerger(string inputFileDirPath, bool skipCleanup, short mergeKParameter)
        {
            _inputFileDirPath = inputFileDirPath;
            _skipCleanup = skipCleanup;
            _mergeKParameter = mergeKParameter;
        }

        public string Merge(IEnumerable<string> filePaths, bool finalRun)
        {
            if (filePaths.Count() > _mergeKParameter)
            {
                var mergedFilesPaths = filePaths.Chunk(filePaths.Count() / _mergeKParameter)
                     .AsParallel()
                     .Select(f => Merge(f, false)).ToList();

                filePaths = mergedFilesPaths;
            }

            string outputFilePath = finalRun ? $"{_inputFileDirPath}\\sorted_{DateTime.UtcNow.ToString("HH_mm_ss")}.txt" : $"{filePaths.First()}_0";

            List<Task> activeTasks = new List<Task>();

            var minHeap = new PriorityQueue<StreamReader, string>(StringComparer.Ordinal);

            using (FileStream fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read, _fileBufferSizeBytes))
            using (BufferedStream bs = new BufferedStream(fs, _fileBufferSizeBytes))
            using (StreamWriter outputFileStream = new StreamWriter(bs))
            {
                foreach (string path in filePaths)
                {
                    var inputFile = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, _fileBufferSizeBytes, FileOptions.SequentialScan);
                    var inputBuffer = new BufferedStream(inputFile, _fileBufferSizeBytes);
                    var inputReader = new StreamReader(inputBuffer);

                    minHeap.Enqueue(inputReader, inputReader.ReadLine());
                }

                StreamReader currentReader;
                string currentRecord;

                while (minHeap.TryDequeue(out currentReader, out currentRecord))
                {
                    outputFileStream.WriteLine(finalRun ? TransformRecord(currentRecord) : currentRecord);
                    var nextRecord = currentReader.ReadLine();
                    if (nextRecord != null)
                    {
                        minHeap.Enqueue(currentReader, nextRecord);
                    }
                    else
                    {
                        var finishedReader = currentReader;
                        activeTasks.Add(Task.Run(() => CleanupFileHandle(finishedReader)));
                    }
                }
            }

            Task.WaitAll(activeTasks.Where(t => !t.IsCompleted).ToArray());

            return outputFilePath;
        }

        private void CleanupFileHandle(StreamReader finishedReader)
        {
            var filePath = ((FileStream)((BufferedStream)finishedReader.BaseStream).UnderlyingStream).Name;
            finishedReader.Close();
            finishedReader.Dispose();

            if (!_skipCleanup)
                File.Delete(filePath);
        }

        private string TransformRecord(string currentRecord)
        {
            var splitedArray = currentRecord.Split(".");
            return $"{splitedArray[1].Trim('-')}.{splitedArray[0]}";
        }
    }
}

