using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LargeFileSorting.Sorting
{
    internal interface IFileMerger
    {
        string Merge(IEnumerable<string> filePaths, bool finalRun);
    }
}
