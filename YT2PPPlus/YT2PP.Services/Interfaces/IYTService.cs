using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace YT2PP.Services.Interfaces
{
    public interface IYTService
    {
        Task<string> GetStreamUrlAsync(string videoUrl);
    }
}
