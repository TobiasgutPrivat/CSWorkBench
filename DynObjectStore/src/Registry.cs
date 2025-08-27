using System.Threading.Tasks;

namespace DynObjectStore;

public class Registry(IDBConnection db)
{
    // like local memory connected to drive (here database-service)

    internal readonly Dictionary<int, WeakReference<RootObject>> objects = [];

    public async Task SaveObject(RootObject rootObject)
    {
        Serializer serializer = new Serializer(rootObject);
        string jsonData = serializer.Serialize(rootObject.root);
        await db.UpdateObject(rootObject.id, jsonData);
    }

    public async Task<RootObject> SaveObject(object obj)
    {
        RootObject rootObject = new RootObject();
        rootObject.root = obj;

        Serializer serializer = new Serializer(rootObject);
        string jsonData = serializer.Serialize(obj);

        Type type = obj.GetType();
        int newId = await db.CreateObject(type.AssemblyQualifiedName!, jsonData);

        objects[newId] = new WeakReference<RootObject>(rootObject);
        rootObject.id = newId;
        rootObject.registry = this;
        return rootObject;
    }

    public async Task DeleteObject(RootObject obj)
    {
        await db.DeleteObject(obj.id);
        objects.Remove(obj.id);
    }

    public async Task DeleteObject(int id)
    {
        await db.DeleteObject(id);
        objects.Remove(id);
    }

    public async Task<RootObject?> GetObject(int id)
    {
        if (objects.TryGetValue(id, out var weakRef))
        {
            if (weakRef.TryGetTarget(out RootObject? existing))
                return existing;

            // collected → remove entry
            objects.Remove(id);
        }

        (string? className, string? data) = await db.GetObject(id);
        if (className == null || data == null) return null;

        Type type = Type.GetType(className) ?? throw new Exception($"Type {className} not found.");

        RootObject rootObject = new RootObject { id = id, registry = this };
        Deserializer deserializer = new Deserializer(rootObject);
        rootObject.root = deserializer.Deserialize(data, type)
                        ?? throw new Exception("Deserialization failed.");

        objects[id] = new WeakReference<RootObject>(rootObject);
        return rootObject;
    }

    public object? ReloadObject(RootObject obj)
    {
        objects.Remove(obj.id);
        return GetObject(obj.id);
    }

    public object? ReloadObject(int id)
    {
        objects.Remove(id);
        return GetObject(id);
    }
}