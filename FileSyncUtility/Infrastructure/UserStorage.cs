using FileSyncUtility.Model;
using FileSyncUtility.Model.Enums;


namespace FileSyncUtility.Infrastructure;

public class UserStorage(string basePath) : IStorageActions
{
    private readonly string _basePath = basePath;

    public void Create(SyncItem syncItem)
    {
        string fullPath = Path.Combine(_basePath, syncItem.RelativePath);

        if (syncItem.Type == ItemType.Folder)
        {
            Directory.CreateDirectory(fullPath);
        }
        else
        {
            File.WriteAllText(fullPath, syncItem.GetData());
        }
    }

    public void Delete(SyncItem syncItem)
    {
        string fullPath = Path.Combine(_basePath, syncItem.RelativePath);

        if (syncItem.Type == ItemType.Folder)
        {
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
        }
        else
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }

    public void Replace(SyncItem syncItem)
    {
        string fullPath = Path.Combine(_basePath, syncItem.RelativePath);

        if (File.Exists(fullPath))
        {
            File.WriteAllText(fullPath, syncItem.GetData());
        }
    }

    public void Rename(SyncItem syncItem)
    {
        string fullPath = Path.Combine(_basePath, syncItem.RelativePath);
        string newPath = Path.Combine(_basePath, syncItem.NewName);

        if (syncItem.Type == ItemType.Folder)
        {
            if (Directory.Exists(fullPath))
            {
                Directory.Move(fullPath, newPath);
            }
        }
        else
        {
            if (File.Exists(fullPath))
            {
                File.Move(fullPath, newPath);
            }
        }
    }
}
