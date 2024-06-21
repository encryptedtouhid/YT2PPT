using System;
using System.Collections.Generic;
using System.Text;

namespace YT2PP.Services.Interfaces
{
    public interface IPPTService
    {
       void CreatePresentation(string outputFilePath, string framesDirectory);
    }
}
