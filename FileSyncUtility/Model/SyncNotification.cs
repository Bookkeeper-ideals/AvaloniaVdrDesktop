using FileSyncUtility.Model.Enums;

namespace FileSyncUtility.Model;

public class SyncNotification
{
    public ItemType Type { get; set; }
    public string Message { get; set; }
}