using FileWatcher.Model.FileLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuxDesign.Api.Services
{
    public interface IFileWatcherService
    {
        LogListResult<FileLog> FileAnalyze(string path);
    }
}
