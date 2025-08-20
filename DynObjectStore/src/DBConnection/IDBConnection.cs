namespace DynObjectStore;

public interface IDBConnection
{
    void Close();
    int CreateObject(string Class, string Data);
    void DeleteObject(int objectId);
    void UpdateObject(int objectId, string newData);
    void GetObject(int objectId, out string? Class, out string? data);
}