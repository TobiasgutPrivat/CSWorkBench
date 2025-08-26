namespace DynObjectStore;

using Newtonsoft.Json;

public class Registry(IDBConnection db)
{
    // like local memory connected to drive (here database-service)

    internal Dictionary<int, object> Objects = [];
    internal Dictionary<object, int> ObjectIds = []; //TODO maybe remove this
    internal Dictionary<object, ObjectReferences> ObjectReferences = []; // rootobject -> subobject -> id (created from serialization)

    public void SaveObject(int id)
    {
        if (Objects.TryGetValue(id, out object? obj))
        {
            Serializer serializer = new Serializer(ObjectReferences[obj]);
            string jsonData = serializer.Serialize(obj);
            db.UpdateObject(id, jsonData);
        }
    }

    public int SaveObject(object obj)
    {
        if (!ObjectReferences.TryGetValue(obj, out ObjectReferences? objRef))
        {
            objRef = new ObjectReferences();
            ObjectReferences[obj] = objRef;
        }
        Serializer serializer = new Serializer(objRef);
        string jsonData = serializer.Serialize(obj);
        if (ObjectIds.TryGetValue(obj, out int id))
        {
            db.UpdateObject(id, jsonData);
            return id;
        }
        else
        {
            Type type = obj.GetType();
            int newId = db.CreateObject(type.AssemblyQualifiedName!, jsonData);
            Objects[newId] = obj;
            ObjectIds[obj] = newId;
            return newId;
        }
    }

    public void DeleteObject(object obj)
    {
        if (ObjectIds.TryGetValue(obj, out int id))
        {
            db.DeleteObject(id);
            Objects.Remove(id);
            ObjectIds.Remove(obj);
            ObjectReferences.Remove(obj);
        }
    }

    public void DeleteObject(int id)
    {
        db.DeleteObject(id);
        Objects.Remove(id);
        if (Objects.TryGetValue(id, out object? obj))
        {
            ObjectIds.Remove(obj);
            ObjectReferences.Remove(obj);
        }
    }

    public object? GetObject(int id)
    {
        if (Objects.TryGetValue(id, out object? value)) return value;

        db.GetObject(id, out string? className, out string? data);

        if (className == null || data == null) return null;

        Type type = Type.GetType(className) ?? throw new Exception($"Type {className} not found.");

        ObjectReferences objRef = new ObjectReferences();
        Deserializer deserializer = new Deserializer(objRef);

        object obj = deserializer.Deserialize(data, type) ?? throw new Exception($"Deserialization failed.");
        Objects[id] = obj;
        ObjectIds[obj] = id;
        ObjectReferences[obj] = objRef;
        return obj;
    }

    public object? GetObject(int rootId, int id)
    {
        object? rootObj = GetObject(rootId);
        if (rootObj == null) return null;
        if (!ObjectReferences.TryGetValue(rootObj, out ObjectReferences? SubObjects)) return null;
        return SubObjects.getSubObject(id);
    }

    public object? ReloadObject(object obj)
    {
        if (ObjectIds.TryGetValue(obj, out int id))
        {
            Objects.Remove(id);
        }
        ObjectIds.Remove(obj);
        ObjectReferences.Remove(obj);
        return GetObject(id);
    }

    public object? ReloadObject(int id)
    {
        if (Objects.TryGetValue(id, out object? obj))
        {
            ObjectIds.Remove(obj);
            ObjectReferences.Remove(obj);
        }
        Objects.Remove(id);
        return GetObject(id);
    }

    public int? GetSubId(object parent, object subObject) => ObjectReferences[parent].getSubObjectId(subObject);

    public Dictionary<string, object>? GetAttachements(object parent, object subObject)
    {
        GetObject(ObjectIds[parent]);
        return ObjectReferences[parent].getAttachements(subObject);
    }

    public void SetAttachment(object parent, object subObject, string name, object obj)
    {
        SaveObject(parent);
        SaveObject(obj);

        ObjectReferences[parent].addAttachement(subObject, name, obj);
    }

    public void DeleteAttachment(object parent, object subObject, string name)
    {
        ObjectReferences[parent].removeAttachement(subObject, name);
        SaveObject(parent);
    }
}