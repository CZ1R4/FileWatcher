using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher.Model.FileLog
{
    public class LogListResult<T>
    {
        public IEnumerable<T>? Data { get; set; }
        public bool DirectoryExists { get; set; }
    }    
}
