using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PuxDesign.Api.Services;
using System.Security.Cryptography;

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
        public ActionResult<List<FileLog>> GetFileLogs(string path)
        {
            var result = fileWatcher.FileAnalyze(path);

            return Ok(result);
        }
    }
}
