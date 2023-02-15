using FileWatcher.Model.FileLog;
using Microsoft.AspNetCore.Mvc;
using PuxDesign.Api.Services;

namespace PuxDesign.Api.Controllers
{
    [ApiController]
    public class FileWatcherController : ControllerBase
    {
        private readonly IFileWatcherService fileWatcher;

        public FileWatcherController(IFileWatcherService fileWatcher)
        {
            this.fileWatcher = fileWatcher;
        }

        [HttpGet]
        [Route("v1/file-watcher")]
        public ActionResult<LogListResult<FileLog>> GetFileLogs(string path)
        {
            var result = fileWatcher.FileAnalyze(path);

            return Ok(result);
        }
    }
}
