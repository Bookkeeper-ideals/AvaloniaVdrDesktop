using FileSyncUtility.Model;
using FileSyncUtility.Model.Enums;
using System.Linq;

namespace FileSyncUtility.Infrastructure
{
    public class FileSystemTracking : ISyncEventsTracking
    {
        private bool _disposed = false;
        private FileSystemWatcher _fileWatcher = null!;
        private FileSystemWatcher _folderWatcher = null!;

        private List<string> _skipEvents = new List<string>();

        public event EventHandler<SyncItem>? SyncEvent;

        public void SkipEvent(string eventUniqueId)
        {
            _skipEvents.Add(eventUniqueId);
        }

        public void Start(string folderPath)
        {
            _fileWatcher = new FileSystemWatcher(folderPath);
            _folderWatcher = new FileSystemWatcher(folderPath);

            _fileWatcher.Changed += OnChanged;
            _fileWatcher.Created += OnCreated;
            _fileWatcher.Deleted += OnDeleted;
            _fileWatcher.Renamed += OnRenamed;
            _fileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;

            _folderWatcher.Created += OnFolderCreated;
            _folderWatcher.Deleted += OnFolderDeleted;
            _folderWatcher.Renamed += OnFolderRenamed;
            _folderWatcher.NotifyFilter = NotifyFilters.DirectoryName;

            _fileWatcher.IncludeSubdirectories = true;
            _fileWatcher.EnableRaisingEvents = true;
            _folderWatcher.IncludeSubdirectories = true;
            _folderWatcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _fileWatcher.Dispose();
            _folderWatcher.Dispose();
        }

        protected virtual void OnCreated(object sender, FileSystemEventArgs e)
        {
            var syncItem = new SyncItem
            {
                Event = FileSystemEvent.Create,
                Type = ItemType.File,
                FullPath = e.FullPath,
                Name = e.Name,
                RelativePath = e.Name
            };

            if (_skipEvents.Remove(syncItem.EventUniqueId))
            {
                return;
            }

            SyncEvent?.Invoke(this, syncItem);
        }

        protected virtual void OnDeleted(object sender, FileSystemEventArgs e)
        {
            var syncItem = new SyncItem
            {
                Event = FileSystemEvent.Delete,
                Type = ItemType.File,
                FullPath = e.FullPath,
                Name = e.Name,
                RelativePath = e.Name
            };

            if (_skipEvents.Remove(syncItem.EventUniqueId))
            {
                return;
            }

            SyncEvent?.Invoke(this, syncItem);
        }

        protected virtual void OnChanged(object sender, FileSystemEventArgs e)
        {
            var syncItem = new SyncItem
            {
                Event = FileSystemEvent.Replace,
                Type = ItemType.File,
                FullPath = e.FullPath,
                Name = e.Name,
                RelativePath = e.Name
            };

            if (_skipEvents.Remove(syncItem.EventUniqueId))
            {
                return;
            }

            SyncEvent?.Invoke(this, syncItem);
        }

        protected virtual void OnRenamed(object sender, RenamedEventArgs e)
        {
            var syncItem = new SyncItem
            {
                Event = FileSystemEvent.Rename,
                Type = ItemType.File,
                FullPath = e.FullPath,
                Name = e.OldName,
                NewName = e.Name,
                RelativePath = e.OldName
            };

            if (_skipEvents.Remove(syncItem.EventUniqueId))
            {
                return;
            }

            SyncEvent?.Invoke(this, syncItem);
        }

        protected virtual void OnFolderCreated(object sender, FileSystemEventArgs e)
        {
            var syncItem = new SyncItem
            {
                Event = FileSystemEvent.Create,
                Type = ItemType.Folder,
                FullPath = e.FullPath,
                Name = e.Name,
                RelativePath = e.Name
            };

            if (_skipEvents.Remove(syncItem.EventUniqueId))
            {
                return;
            }

            SyncEvent?.Invoke(this, syncItem);
        }

        protected virtual void OnFolderDeleted(object sender, FileSystemEventArgs e)
        {
            var syncItem = new SyncItem
            {
                Event = FileSystemEvent.Delete,
                Type = ItemType.Folder,
                FullPath = e.FullPath,
                Name = e.Name,
                RelativePath = e.Name
            };

            if (_skipEvents.Remove(syncItem.EventUniqueId))
            {
                return;
            }

            SyncEvent?.Invoke(this, syncItem);
        }

        protected virtual void OnFolderRenamed(object sender, RenamedEventArgs e)
        {
            var syncItem = new SyncItem
            {
                Event = FileSystemEvent.Rename,
                Type = ItemType.Folder,
                FullPath = e.FullPath,
                Name = e.OldName,
                NewName = e.Name,
                RelativePath = e.OldName
            };

            if (_skipEvents.Remove(syncItem.EventUniqueId))
            {
                return;
            }

            SyncEvent?.Invoke(this, syncItem);
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
                _fileWatcher.Changed -= OnChanged;
                _fileWatcher.Created -= OnCreated;
                _fileWatcher.Deleted -= OnDeleted;
                _fileWatcher.Renamed -= OnRenamed;

                _folderWatcher.Created -= OnFolderCreated;
                _folderWatcher.Deleted -= OnFolderDeleted;
                _folderWatcher.Renamed -= OnFolderRenamed;
                _folderWatcher.Deleted -= OnFolderDeleted;

                _fileWatcher.Dispose();
                _folderWatcher.Dispose();
            }

            _disposed = true;
        }
    }
}
