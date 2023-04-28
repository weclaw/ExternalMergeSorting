using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LargeFileSorting.InputArguments
{
    internal struct SortingParameters
    {
        public SortingParameters(string inputFilePath, int chunkLinesLimit, int memoryUsageMBSoftLimit)
        {
            InputFilePath = inputFilePath;
            ChunkLinesLimit = chunkLinesLimit;
            MemoryUsageMBSoftLimit = memoryUsageMBSoftLimit;
        }

        public string InputFilePath { get; set; }
        public int ChunkLinesLimit { get; set; }
        public int MemoryUsageMBSoftLimit { get; set; }  
    }
}
