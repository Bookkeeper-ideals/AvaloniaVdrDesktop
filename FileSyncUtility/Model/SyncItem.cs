using FileSyncUtility.Model.Enums;

using System.IO;

namespace FileSyncUtility.Model;

public class SyncItem
{
    public ItemType Type { get; set; }
    public FileSystemEvent Event { get; set; }
    public string FullPath { get; set; }
    public string? Name { get; set; }
    public string? NewName { get; set; }
    public string? RelativePath { get; set; }
    public string EventUniqueId => $"{Event}{RelativePath}";

    public string? GetData()
    {
        if (Type == ItemType.File &&
            (Event == FileSystemEvent.Create || Event == FileSystemEvent.Replace))
        {
            try
            {
                return File.ReadAllText(FullPath);
            }
            catch (Exception ex)
            {
                // Log or handle exceptions as needed
                return null;
            }
        }

        return null;
    }
}
