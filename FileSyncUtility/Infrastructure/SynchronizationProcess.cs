using FileSyncUtility.Model;
using FileSyncUtility.Model.Enums;

namespace FileSyncUtility.Infrastructure;

public class SynchronizationProcess
{
    private readonly ISyncEventsTracking _vdrSyncEvents;
    private readonly ISyncEventsTracking _storageSyncEvents;
    private readonly IStorageActions _vdrCloudStorage;
    private readonly IStorageActions _userStorage;

    public event EventHandler<SyncNotification>? ProcessNotification;

    public SynchronizationProcess()
    {
        _vdrSyncEvents = new FileSystemTracking();
        _storageSyncEvents = new FileSystemTracking();
        _vdrCloudStorage = new UserStorage();
        _userStorage = new UserStorage();
    }

    public void Start(string masterFolderPath, string syncFolderPath)
    {
        _vdrSyncEvents.SyncEvent += OnVdrEvent;
        _storageSyncEvents.SyncEvent += OnStorageEvent;

        _vdrSyncEvents.Start(masterFolderPath);
        _storageSyncEvents.Start(syncFolderPath);
    }

    protected virtual void OnVdrEvent(object? sender, SyncItem e)
    {
        _storageSyncEvents.SkipEvent(e.EventUniqueId);

        switch (e.Event)
        {
            case FileSystemEvent.Create:
                _userStorage.Create(e);
                break;
            case FileSystemEvent.Delete:
                _userStorage.Delete(e);
                break;
            case FileSystemEvent.Replace:
                _userStorage.Replace(e);
                break;
            case FileSystemEvent.Rename:
                _userStorage.Rename(e);
                break;
        }

        SendNotification(e.Type, $"FileStorage: {e.Type} {e.Event} successful");
    }

    protected virtual void OnStorageEvent(object? sender, SyncItem e)
    {
        _vdrSyncEvents.SkipEvent(e.EventUniqueId);

        switch (e.Event)
        {
            case FileSystemEvent.Create:
                _vdrCloudStorage.Create(e);
                break;
            case FileSystemEvent.Delete:
                _vdrCloudStorage.Delete(e);
                break;
            case FileSystemEvent.Replace:
                _vdrCloudStorage.Replace(e);
                break;
            case FileSystemEvent.Rename:
                _vdrCloudStorage.Rename(e);
                break;
        }
    }

    private void SendNotification(ItemType type, string message)
    {
        var notification = new SyncNotification
        {
            Type = type,
            Message = message
        };
        ProcessNotification?.Invoke(this, notification);
    }
}