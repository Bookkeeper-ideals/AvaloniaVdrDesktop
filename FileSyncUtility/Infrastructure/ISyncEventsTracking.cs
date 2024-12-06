using FileSyncUtility.Model;
using FileSyncUtility.Model.Enums;

namespace FileSyncUtility.Infrastructure;

public interface ISyncEventsTracking : IDisposable
{
    public event EventHandler<SyncItem>? SyncEvent;

    void Start(string folderPath);
    void Stop();
    void SkipEvent(string eventUniqueId);
}