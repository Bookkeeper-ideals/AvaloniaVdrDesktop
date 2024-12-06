using FileSyncUtility.Model;

namespace FileSyncUtility.Infrastructure;

public interface IStorageActions
{
    public void Create(SyncItem syncItem);
    public void Delete(SyncItem syncItem);
    public void Replace(SyncItem syncItem);
    public void Rename(SyncItem syncItem);
}