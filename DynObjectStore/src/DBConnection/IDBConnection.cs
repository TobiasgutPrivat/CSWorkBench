namespace DynObjectStore;

public interface IDBConnection
{
    Task Dispose();
    Task Open();
    Task<int> CreateObject(string typeName, string jsonData);
    Task UpdateObject(int id, string jsonData);
    Task DeleteObject(int id);
    Task<(string? typeName, string? jsonData)> GetObject(int id);
}