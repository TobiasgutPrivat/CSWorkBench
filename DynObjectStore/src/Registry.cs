namespace DynObjectStore;

public class Registry(IDBConnection db)
{
    // like local memory connected to drive (here database-service)

    internal readonly Dictionary<int, WeakReference<RootObject>> objects = [];

    public void SaveObject(RootObject rootObject)
    {
        Serializer serializer = new Serializer(rootObject);
        string jsonData = serializer.Serialize(rootObject.root);
        db.UpdateObject(rootObject.id, jsonData);
    }

    public RootObject SaveObject(object obj)
    {
        RootObject rootObject = new RootObject();
        rootObject.root = obj;

        Serializer serializer = new Serializer(rootObject);
        string jsonData = serializer.Serialize(obj);

        Type type = obj.GetType();
        int newId = db.CreateObject(type.AssemblyQualifiedName!, jsonData);

        objects[newId] = new WeakReference<RootObject>(rootObject);
        rootObject.id = newId;
        rootObject.registry = this;
        return rootObject;
    }

    public void DeleteObject(RootObject obj)
    {
        db.DeleteObject(obj.id);
        objects.Remove(obj.id);
    }

    public void DeleteObject(int id)
    {
        db.DeleteObject(id);
        objects.Remove(id);
    }

    public RootObject? GetObject(int id)
    {
        if (objects.TryGetValue(id, out var weakRef))
        {
            if (weakRef.TryGetTarget(out RootObject? existing))
                return existing;

            // collected → remove entry
            objects.Remove(id);
        }

        db.GetObject(id, out string? className, out string? data);
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