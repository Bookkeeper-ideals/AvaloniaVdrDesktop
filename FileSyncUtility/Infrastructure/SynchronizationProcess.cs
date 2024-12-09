using FileSyncUtility.Model;
using FileSyncUtility.Model.Enums;

namespace FileSyncUtility.Infrastructure;

public class SynchronizationProcess : IDisposable
{
    private bool _disposed = false;
    private readonly ISyncEventsTracking _vdrSyncEvents;
    private readonly ISyncEventsTracking _storageSyncEvents;
    private IStorageActions _vdrCloudStorage;
    private IStorageActions _userStorage;

    public event EventHandler<SyncNotification>? ProcessNotification;

    public SynchronizationProcess()
    {
        _vdrSyncEvents = new FileSystemTracking();
        _storageSyncEvents = new FileSystemTracking();
    }

    public void Start(string masterFolderPath, string syncFolderPath)
    {
        _vdrCloudStorage = new UserStorage(masterFolderPath);
        _userStorage = new UserStorage(syncFolderPath);

        _vdrSyncEvents.SyncEvent += OnVdrEvent;
        _storageSyncEvents.SyncEvent += OnStorageEvent;

        _vdrSyncEvents.Start(masterFolderPath);
        _storageSyncEvents.Start(syncFolderPath);
    }

    protected virtual void OnVdrEvent(object? sender, SyncItem e)
    {
        if (e.Event == FileSystemEvent.Rename)
        {
            var deleteEvent = new SyncItem
            {
                Event = FileSystemEvent.Delete,
                RelativePath = e.NewName
            };
            _storageSyncEvents.SkipEvent(deleteEvent.EventUniqueId);
            var createEvent = new SyncItem
            {
                Event = FileSystemEvent.Create,
                RelativePath = e.NewName
            };
            _storageSyncEvents.SkipEvent(createEvent.EventUniqueId);
        }
        else
        {
            _storageSyncEvents.SkipEvent(e.EventUniqueId);
        }
        
        if (e.Event == FileSystemEvent.Replace)
            _storageSyncEvents.SkipEvent(e.EventUniqueId);

        try
        {
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
        }
        catch (Exception ex)
        {
            SendNotification(e.Type, $"FileStorage: {e.Type} {e.Event} failed: {ex.Message}");
        }

        SendNotification(e.Type, $"FileStorage: {e.Type} {e.Event} successful");
    }

    protected virtual void OnStorageEvent(object? sender, SyncItem e)
    {
        if (e.Event == FileSystemEvent.Rename)
        {
            var deleteEvent = new SyncItem
            {
                Event = FileSystemEvent.Delete,
                RelativePath = e.NewName
            };
            _vdrSyncEvents.SkipEvent(deleteEvent.EventUniqueId);
            var createEvent = new SyncItem
            {
                Event = FileSystemEvent.Create,
                RelativePath = e.NewName
            };
            _vdrSyncEvents.SkipEvent(createEvent.EventUniqueId);
        }
        else
        {
            _vdrSyncEvents.SkipEvent(e.EventUniqueId);
        }
        
        if (e.Event == FileSystemEvent.Replace)
            _vdrSyncEvents.SkipEvent(e.EventUniqueId);

        try
        {
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
        catch (Exception ex)
        {
            SendNotification(e.Type, $"VdrCloud: {e.Type} {e.Event} failed: {ex.Message}");
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

    public void Dispose()
    {
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _vdrSyncEvents.SyncEvent -= OnVdrEvent;
            _storageSyncEvents.SyncEvent -= OnStorageEvent;

            _vdrSyncEvents.Dispose();
            _storageSyncEvents.Dispose();
        }

        _disposed = true;
    }
}