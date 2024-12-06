using FileSyncUtility.Model.Enums;

namespace FileSyncUtility.Model;

public class SyncItem
{
    public ItemType Type { get; set; }
    public FileSystemEvent Event { get; set; }
    public string FullPath { get; set; }
    public string? Name { get; set; }
    public string? RelativePath { get; set; }
    public string EventUniqueId => $"{Event}{FullPath}";

    public Stream? GetDataStream()
    {
        if (Type == ItemType.File &&
            (Event == FileSystemEvent.Create || Event == FileSystemEvent.Replace))
        {
            throw new NotImplementedException();
        }

        return null;
    }
}