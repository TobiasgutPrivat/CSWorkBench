namespace DynObjectStore;

public class Registry(IDBConnection db)
{
    // like local memory connected to drive (here database-service)

    internal Dictionary<int, RootObject> Objects = [];

    public void SaveObject(RootObject obj)
    {
        Serializer serializer = new Serializer(obj);
        string jsonData = serializer.Serialize(obj);
        db.UpdateObject(obj.id, jsonData);
    }

    public int SaveObject(object obj)
    {
        RootObject objWrap = new RootObject();
        objWrap.root = obj;

        Serializer serializer = new Serializer(objWrap);
        string jsonData = serializer.Serialize(obj);

        Type type = obj.GetType();
        int newId = db.CreateObject(type.AssemblyQualifiedName!, jsonData);

        Objects[newId] = objWrap;
        objWrap.id = newId;
        return newId;
    }

    public void DeleteObject(RootObject obj)
    {
        db.DeleteObject(obj.id);
        Objects.Remove(obj.id);
    }

    public void DeleteObject(int id)
    {
        db.DeleteObject(id);
        Objects.Remove(id);
    }

    public RootObject? GetObject(int id)
    {
        if (Objects.TryGetValue(id, out RootObject? value)) return value;

        db.GetObject(id, out string? className, out string? data);

        if (className == null || data == null) return null;

        Type type = Type.GetType(className) ?? throw new Exception($"Type {className} not found.");

        RootObject rootObject = new RootObject();
        rootObject.id = id;
        rootObject.registry = this;
        Deserializer deserializer = new Deserializer(rootObject);

        rootObject.root = deserializer.Deserialize(data, type) ?? throw new Exception($"Deserialization failed.");
        Objects[id] = rootObject;
        return rootObject;
    }

    public object? ReloadObject(RootObject obj)
    {
        Objects.Remove(obj.id);
        return GetObject(obj.id);
    }

    public object? ReloadObject(int id)
    {
        Objects.Remove(id);
        return GetObject(id);
    }
}