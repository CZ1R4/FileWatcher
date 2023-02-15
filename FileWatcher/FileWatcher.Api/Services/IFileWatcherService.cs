using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuxDesign.Api.Services
{
    public interface IFileWatcherService
    {
        List<FileLog> FileAnalyze(string path);
    }
}
