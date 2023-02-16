using FileWatcher.Model.FileLog;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace PuxDesign.Api.Services
{
    public class FileWatcherService : IFileWatcherService
    {
        public LogListResult<FileLog> FileAnalyze(string path)
        {
            path = FormatInputPath(path) +"/"; //pro sjednocen� form�tu adresy p�id�m "/"
            
            DirectoryInfo directory = new DirectoryInfo(path);

            if(!directory.Exists ) 
            {
                return new LogListResult<FileLog> { Data = null, DirectoryExists= false };
            }

            path = directory.FullName;

            var currentPath = Directory.GetCurrentDirectory();
            var jsonLogPath = Path.Combine(currentPath, "file_logs.json");

            //vytvo�en� json souboru pro sb�r zm�n v zadan�m souboru , pokud neexistuje
            if (!System.IO.File.Exists(jsonLogPath))
            {
                using (FileStream fs = System.IO.File.Create(jsonLogPath))
                {
                }

                Console.WriteLine("log file created");
            }

            //z�sk�n� sou�asn�ch informac� o zadan�m adres��i
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

            //zisk�n� star�ch informac� o zadan�m adres��i
            var jsonData = System.IO.File.ReadAllText(jsonLogPath);
            var deserializedJsonData = new List<FileLog>();

            if (jsonData != "") deserializedJsonData = JsonConvert.DeserializeObject<List<FileLog>>(jsonData); //kompletni json data

            var historyDirectoryData = deserializedJsonData
                .Where(x => x.RootDirectoryAddress == path)
                .ToList();

            var result = new List<FileLog>();

            //detekce opraven�ch a nov�ch soubor�
            foreach (var log in currentDirectoryData)
            {
                var isFileNew = !historyDirectoryData.Any(x => x.FileAddress == log.FileAddress) && historyDirectoryData.Count > 0;
                var isFileModified = !isFileNew &&
                    historyDirectoryData.FirstOrDefault(x => x.FileAddress == log.FileAddress)?.ComputedHash != null &&
                    log.ComputedHash != historyDirectoryData.FirstOrDefault(x => x.FileAddress == log.FileAddress)?.ComputedHash;

                //P�id�n soubor
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

                //Zm�n�n soubor
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

                //pokud ��dn� z podm�nek nevyhovuje mus�m ulo�it do result abych zachoval historii
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

            //detekce smazan�ho souboru
            var deletedFiles = historyDirectoryData.Where(x => !currentDirectoryData.Select(c => c.FileAddress).Contains(x.FileAddress)).ToList();
            foreach (var log in deletedFiles)
            {
                //Odebr�n soubor 
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

            //vymazani historie dan�ho adres��e
            foreach (var log in deserializedJsonData.Where(x => x.RootDirectoryAddress == path).ToList())
            {
                deserializedJsonData.Remove(log);
            }

            //obnova nov� historie
            foreach (var log in result.Where(x => x.Status != FileStatus.Deleted))
            {
                deserializedJsonData.Add(log);
            }

            //inicializace historie adres��e pokud neexistuje
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
                // Otev�ete soubor a vypo�te jeho hash hodnotu
                using (var stream = System.IO.File.OpenRead(path))
                {
                    var hash = sha256.ComputeHash(stream);

                    // P�evede hash hodnotu na �et�zec
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
