namespace FileWatcher.Model.FileLog
{
    public class FileLog
    {
        public string RootDirectoryAddress { get; set; }
        public string FileName { get; set; }
        public string FileAddress { get; set; }
        public int? FileVersion { get; set; }
        public FileStatus Status { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string ComputedHash { get; set; }
    }
}