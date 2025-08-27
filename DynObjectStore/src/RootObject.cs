namespace DynObjectStore;

public class RootObject
{
    // represents a full object graph based on one root object
    // stores id's of objects during deserialization to use same id's for serialization (allows more consistent identification)
    // also manages attachments

    public Registry registry = null!; // home registry
    public object root = null!;
    public int id;
    private Dictionary<object, int> SubObjectIds = [];
    private Dictionary<int, object> SubObjects = [];
    private Dictionary<object, Dictionary<string, RootObject>> Attachements = []; // maybe into registry
    internal int nextId = 0;

    public void addSubObject(object obj, int nextId)
    {
        SubObjectIds[obj] = nextId;
        SubObjects[nextId] = obj;
    }

    // public void removeSubObject(object obj) // probably not needed
    // {
    //     SubObjectIds.Remove(obj);
    //     SubObjects.Remove(SubObjectIds[obj]);
    // }

    public object? getSubObject(int id)
    {
        if (SubObjects.TryGetValue(id, out var obj))
        {
            return obj;
        }
        return null;
    }

    public int? getSubObjectId(object obj)
    {
        if (SubObjectIds.TryGetValue(obj, out var id))
        {
            return id;
        }
        return null;
    }

    public void addAttachement(object obj, string name, RootObject attachement)
    {
        if (!Attachements.TryGetValue(obj, out var attachments))
        {
            attachments = new Dictionary<string, RootObject>();
            Attachements[obj] = attachments;
        }
        attachments[name] = attachement;
    }
    public void addAttachement(object obj, string name, object attachement)
    {
        addAttachement(obj, name, registry.SaveObject(attachement));
    }

    internal void setAttachements(object obj, Dictionary<string, RootObject> value)
    {
        Attachements[obj] = value;
    }

    public void removeAttachement(object obj, string name)
    {
        if (Attachements.TryGetValue(obj, out var attachments))
        {
            attachments.Remove(name);
        }
    }

    public Dictionary<string, RootObject>? getAttachements(object obj)
    {
        if (SubObjectIds.TryGetValue(obj, out var attachments))
        {
            return Attachements[obj];
        }
        return null;
    }

    // internal for serialization
    internal int registerSubObject(object obj, int id)
    {
        if (getSubObject(id) != null)
        {
            throw new Exception($"Object with ID {id} already exists.");
        }
        nextId = Math.Max(nextId, id + 1);
        SubObjectIds[obj] = id;
        SubObjects[id] = obj;
        return id;
    }

    internal int registerSubObject(object obj)
    {
        if (SubObjectIds.TryGetValue(obj, out var id))
        {
            return id;
        }
        else
        {
            id = nextId;
            nextId++;
            SubObjectIds[obj] = id;
            SubObjects[id] = obj;
            Attachements[obj] = new Dictionary<string, RootObject>();
            return id;
        }
    }
}