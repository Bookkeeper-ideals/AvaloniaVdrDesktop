using FileSyncUtility.Model.Enums;

namespace FileSyncUtility.Model;

public class SkipEvent
{
    public FileSystemEvent Type { get; set; }
    public string FullPath { get; set; }
}