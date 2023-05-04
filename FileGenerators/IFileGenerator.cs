using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LargeFileSorting.FileGenerators
{
    internal interface IFileGenerator
    {
        int Generate();
    }
}
