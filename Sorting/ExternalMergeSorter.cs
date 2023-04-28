using LargeFileSorting.InputArguments;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;

namespace LargeFileSorting.Sorting
{
    internal struct ExternalMergeSorter : IFileSorter
    {
        private const int BYTES_IN_MG = 1024 * 1024;
        private const short LONG_MAX_LENGTH = 13;
        private readonly string _inputFilePath;
        private readonly int _inputFileBufferSizeBytes = 50000; //FileShare.None ucina 4s, lepsze niz 64, to i 100000 sweet spot, ale jak pamiec wazniejsza 10000 nie jest duzo slabsze a zuzywa 10/ mniej pamieci i pewnie rozjedzie sie na k-way mergu
        private readonly string _outputFilePathBase;
        private readonly int _splitChunkSize;
        private readonly long _memoryLimitBytes;

        public ExternalMergeSorter(SortingParameters sortingParameters) : this()
        {
            _inputFilePath = sortingParameters.InputFilePath;
            _outputFilePathBase = _inputFilePath.Split(".")[0].Replace("ExternalMergeSort", "ExternalMergeSort\\temp");
            _splitChunkSize = 1000000;// sortingParameters.ChunkLinesLimit; (chyba ustawic limit na 100000)
            _memoryLimitBytes = sortingParameters.MemoryUsageMBSoftLimit * BYTES_IN_MG;
        }

        public void Sort()
        {
            //using (FileStream fs = new FileStream(_inputFilePath, FileMode.Open, FileAccess.Read, FileShare.None, _inputFileBufferSizeBytes, FileOptions.SequentialScan))
            //using (BufferedStream bs = new BufferedStream(fs, _inputFileBufferSizeBytes))
            //using (StreamReader sr = new StreamReader(bs))
            //{
            //    using (FileStream fs2 = new FileStream(_outputFilePathBase.Replace("2GB", "20GB"), FileMode.Create, FileAccess.Write, FileShare.Read, _inputFileBufferSizeBytes))
            //    using (BufferedStream bs2 = new BufferedStream(fs2, _inputFileBufferSizeBytes))
            //    using (StreamWriter outputFileStream = new StreamWriter(bs2))
            //    {
            //        string line;

            //        for(int i = 0 ; i < 10;i++)
            //        {
            //            while ((line = sr.ReadLine()) != null)
            //            {
            //                outputFileStream.WriteLine(line);
            //            }

            //            sr.DiscardBufferedData();
            //            sr.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
            //        }
            //    }


            //}

            var iterations = Split();
            Merge(iterations);
        }

        private int Split()
        {
            using (FileStream fs = new FileStream(_inputFilePath, FileMode.Open, FileAccess.Read, FileShare.None, _inputFileBufferSizeBytes, FileOptions.SequentialScan))
            using (BufferedStream bs = new BufferedStream(fs, _inputFileBufferSizeBytes))
            using (StreamReader sr = new StreamReader(bs))
            {
                string outputFilePathBase = _outputFilePathBase;
                long memoryLimitBytes = _memoryLimitBytes;
                int iteration = 0;
                List<Task> sortAndSaveTasks = new List<Task>();
                Process proc = Process.GetCurrentProcess();

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

                    //is this issue?
                    var sortInputParams = new SortAndSaveInputParameters(outputFilePathBase, iteration++, array);
                    sortAndSaveTasks.Add(Task.Run(() => SortAndSaveArray(sortInputParams)));
                }

                Task.WaitAll(sortAndSaveTasks.Where(t => !t.IsCompleted).ToArray());

                return iteration;
            }

            string TransormRecord(string line)
            { 
                var splitedArray = line.Split(".");
                return $"{splitedArray[1]}.{splitedArray[0].PadLeft(LONG_MAX_LENGTH, '-')}";
            }

            void SortAndSaveArray(SortAndSaveInputParameters input)
            {
                var fileName = $"{input.OutputFilePathBase}_{input.Iteration}.txt";

                Array.Sort(input.InputArray);
                File.WriteAllLines(fileName, input.InputArray);
            }
        }

        private void Merge(int iterations)
        {
            int readingBufferSize = 50000;
            string outputFilePathBase = _outputFilePathBase;

            var minHeap = new PriorityQueue<StreamReader, string>();

            using (FileStream fs = new FileStream($"{_outputFilePathBase}_final.txt", FileMode.Create, FileAccess.Write, FileShare.Read, _inputFileBufferSizeBytes))
            using (BufferedStream bs = new BufferedStream(fs, _inputFileBufferSizeBytes))
            using (StreamWriter outputFileStream = new StreamWriter(bs))
            {
                //var streamsArray = new StreamReader[iterations];
                //var recordsBuffer = new string[iterations];
                ////rozwazyc pozniej, 3 opcje - najpierw otworzy
                //Parallel.For(0, iterations, i =>
                //{
                //    var fs = new FileStream($"{outputFilePathBase}_{i}.txt", FileMode.Open, FileAccess.Read, FileShare.None, readingBufferSize, FileOptions.SequentialScan);
                //    var bs = new BufferedStream(fs, readingBufferSize);
                //    var sr = new StreamReader(bs);
                //    recordsBuffer[i] = sr.ReadLine();
                //    streamsArray[i] = sr;

                //    //lock (_heapSync)
                //    //{
                //    //    minHeap.Enqueue(sr, sr.ReadLine());
                //    //}
                //});

                //for (int i = 0; i < iterations; i++)
                //{
                //    minHeap.Enqueue(streamsArray[i], recordsBuffer[i]);
                //}

                long counter = 0;

                for (int i = 0; i < iterations; i++)
                {
                    var inputFile = new FileStream($"{outputFilePathBase}_{i}.txt", FileMode.Open, FileAccess.Read, FileShare.None, readingBufferSize, FileOptions.SequentialScan);
                    var inputBuffer = new BufferedStream(inputFile, readingBufferSize);
                    var inputReader = new StreamReader(inputBuffer);

                    minHeap.Enqueue(inputReader, inputReader.ReadLine());
                    counter++;
                }

                StreamReader currentReader;
                string currentRecord;

                while (minHeap.TryDequeue(out currentReader, out currentRecord))
                {
                    counter++;
                    //Try async?
                    outputFileStream.WriteLine(TransformRecord(currentRecord));
                    var nextRecord = currentReader.ReadLine();
                    if(nextRecord != null)
                    {
                        minHeap.Enqueue(currentReader, nextRecord);
                    }
                    else
                    {
                        currentReader.Close();
                        currentReader.Dispose();
                    }
                }
            }

            //delete files

            // minheap tuple string bufferedReader - then compare if instead of stream in queue we keep index in the array of it
            // jak nie zadziala za dobrze to moze byc kilkukrotny k-merge, parallel i potem wynikowe znowu az zostanie plikow mniej niz k (wpisac w usprawnienia jesli nie bedzie potrzebny, albo 2 tryb)  
            // filestream write at first. Then compare to buffer in array to be written parallel when full
        }

        private string TransformRecord(string currentRecord)
        {
            var splitedArray = currentRecord.Split(".");
            return $"{splitedArray[1].Trim('-')}.{splitedArray[0]}";
        }
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