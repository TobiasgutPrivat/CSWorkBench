namespace DynObjectStore;

public interface IDBConnection
{
    void Close();
    int CreateObject(string Class, string Data);
    void DeleteObject(int objectId);
    void UpdateObject(int objectId, string newData);
    void GetObject(int objectId, out string? Class, out string? data);
    public void CreateAttachment(int parentId, string path, string name, int objectId);
    public List<Tuple<string, string, int>> GetAttachments(int objectId);
    void DeleteAttachment(int parentId, string path, string name);
}