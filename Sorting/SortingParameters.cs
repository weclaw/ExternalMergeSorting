using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LargeFileSorting.Sorting
{
    internal struct SortingParameters
    {
        public SortingParameters(string inputFilePath, 
            int chunkLinesLimit,
            bool skipCleanup,
            short mergeKParameter)
        {
            InputFilePath = inputFilePath;
            ChunkLinesLimit = chunkLinesLimit;
            SkipCleanup = skipCleanup;
            MergeKParameter = mergeKParameter;
        }

        public string InputFilePath { get; private set; }
        public int ChunkLinesLimit { get; private set; }
        public bool SkipCleanup { get; private set; }
        public short MergeKParameter { get; private set; }
    }
}
