using FileWatcher.Model.FileLog;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace PuxDesign.Api.Services
{
    public class FileWatcherService : IFileWatcherService
    {
        public LogListResult<FileLog> FileAnalyze(string path)
        {
            path = FormatInputPath(path) +"/"; //pro sjednocení formátu adresy pøidám "/"
            
            DirectoryInfo directory = new DirectoryInfo(path);

            if(!directory.Exists ) 
            {
                return new LogListResult<FileLog> { Data = null, DirectoryExists= false };
            }

            path = directory.FullName;

            var currentPath = Directory.GetCurrentDirectory();
            var jsonLogPath = Path.Combine(currentPath, "file_logs.json");

            //vytvoøení json souboru pro sbìr zmìn v zadaným souboru , pokud neexistuje
            if (!System.IO.File.Exists(jsonLogPath))
            {
                using (FileStream fs = System.IO.File.Create(jsonLogPath))
                {
                }

                Console.WriteLine("log file created");
            }

            //získání souèasných informací o zadaném adresáøi
            var currentDirectoryData = new List<FileLog>();
            foreach (var file in directory.GetFiles("*", SearchOption.AllDirectories))
            {
                var fileLog = new FileLog()
                {
                    RootDirectoryAddress = path,
                    FileName = file.FullName.Replace(path, ""),
                    FileAddress = file.FullName,
                    LastWriteTime = file.LastWriteTime,
                    FileVersion = 1,
                    Status = FileStatus.Original,
                    ComputedHash = ComputeHashFile(file.FullName)
                };

                currentDirectoryData.Add(fileLog);
            }

            //ziskání starých informací o zadaném adresáøi
            var jsonData = System.IO.File.ReadAllText(jsonLogPath);
            var deserializedJsonData = new List<FileLog>();

            if (jsonData != "") deserializedJsonData = JsonConvert.DeserializeObject<List<FileLog>>(jsonData); //kompletni json data

            var historyDirectoryData = deserializedJsonData
                .Where(x => x.RootDirectoryAddress == path)
                .ToList();

            var result = new List<FileLog>();

            //detekce opravených a nových souborù
            foreach (var log in currentDirectoryData)
            {
                var isFileNew = !historyDirectoryData.Any(x => x.FileAddress == log.FileAddress) && historyDirectoryData.Count > 0;
                var isFileModified = !isFileNew &&
                    historyDirectoryData.FirstOrDefault(x => x.FileAddress == log.FileAddress)?.ComputedHash != null &&
                    log.ComputedHash != historyDirectoryData.FirstOrDefault(x => x.FileAddress == log.FileAddress)?.ComputedHash;

                //Pøidán soubor
                if (isFileNew)
                {
                    var newFile = currentDirectoryData.FirstOrDefault(x => x.FileAddress == log.FileAddress);

                    result.Add(new FileLog
                    {
                        RootDirectoryAddress = path,
                        FileName = newFile.FileName,
                        FileAddress = newFile.FileAddress,
                        FileVersion = newFile.FileVersion,
                        LastWriteTime = newFile.LastWriteTime,
                        Status = FileStatus.Added,
                        ComputedHash = ComputeHashFile(newFile.FileAddress)
                    });

                    continue;
                }

                //Zmìnìn soubor
                if (isFileModified)
                {
                    var modifiedFile = currentDirectoryData.FirstOrDefault(x => x.FileAddress == log.FileAddress);

                    result.Add(new FileLog
                    {
                        RootDirectoryAddress = path,
                        FileName = modifiedFile.FileName,
                        FileAddress = modifiedFile.FileAddress,
                        FileVersion = modifiedFile.FileVersion + 1,
                        LastWriteTime = modifiedFile.LastWriteTime,
                        Status = FileStatus.Modified,
                        ComputedHash = modifiedFile.ComputedHash,
                    });

                    continue;
                }

                //pokud žádná z podmínek nevyhovuje musím uložit do result abych zachoval historii
                result.Add(new FileLog
                {
                    RootDirectoryAddress = path,
                    FileName = log.FileName,
                    FileAddress = log.FileAddress,
                    FileVersion = log.FileVersion,
                    LastWriteTime = log.LastWriteTime,
                    Status = FileStatus.Original,
                    ComputedHash = log.ComputedHash,
                });

            }

            //detekce smazaného souboru
            var deletedFiles = historyDirectoryData.Where(x => !currentDirectoryData.Select(c => c.FileAddress).Contains(x.FileAddress)).ToList();
            foreach (var log in deletedFiles)
            {
                //Odebrán soubor 
                result.Add(new FileLog
                {
                    RootDirectoryAddress = path,
                    FileName = log.FileName,
                    FileAddress = log.FileAddress,
                    FileVersion = log.FileVersion,
                    LastWriteTime = log.LastWriteTime,
                    Status = FileStatus.Deleted,
                    ComputedHash = log.ComputedHash,
                });
            }

            //vymazani historie daného adresáøe
            foreach (var log in deserializedJsonData.Where(x => x.RootDirectoryAddress == path).ToList())
            {
                deserializedJsonData.Remove(log);
            }

            //obnova nové historie
            foreach (var log in result.Where(x => x.Status != FileStatus.Deleted))
            {
                deserializedJsonData.Add(log);
            }

            //inicializace historie adresáøe pokud neexistuje
            if (!historyDirectoryData.Any())
            {
                foreach (var log in currentDirectoryData)
                {
                    deserializedJsonData.Add(log);
                }
            }

            var serializedJsonData = JsonConvert.SerializeObject(deserializedJsonData);

            System.IO.File.WriteAllText(jsonLogPath, serializedJsonData);

            return new LogListResult<FileLog> { 
                Data = result.GroupBy(x => x.ComputedHash).Select(g => g.First()).ToList(), 
                DirectoryExists = true }; 
        }

        private string ComputeHashFile(string path)
        {
            string hashString;
            using (var sha256 = SHA256.Create())
            {
                // Otevøete soubor a vypoète jeho hash hodnotu
                using (var stream = System.IO.File.OpenRead(path))
                {
                    var hash = sha256.ComputeHash(stream);

                    // Pøevede hash hodnotu na øetìzec
                    hashString = BitConverter.ToString(hash);
                }
            }

            return hashString;
        }

        private string FormatInputPath(string path)
        {
            var currentPath = Directory.GetCurrentDirectory();
            var formatedPath = path.Replace("~",currentPath);

            return formatedPath;
        }
    }
}
