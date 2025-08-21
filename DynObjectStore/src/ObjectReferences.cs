public class ObjectReferences
{
    // stores id's of objects during deserialization to use same id's for serialization (allows more consistent identification)
    // also manages attachments

    private Dictionary<object, int> SubObjectIds = [];
    private Dictionary<int, object> SubObjects = [];
    private Dictionary<object, Dictionary<string, object>> Attachements = []; // maybe into registry
    public int nextId = 0;

    public void addSubObject(object obj, int nextId)
    {
        SubObjectIds[obj] = nextId;
        SubObjects[nextId] = obj;
    }

    public void removeSubObject(object obj)
    {
        SubObjectIds.Remove(obj);
        SubObjects.Remove(SubObjectIds[obj]);
    }

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

    internal int registerSubObject(object obj, int id)
    {
        if (getSubObject(id) != null)
        {
            throw new Exception($"Object with ID {id} already exists.");
        }
        nextId = Math.Max(nextId, id + 1);
        SubObjectIds[obj] = id;
        SubObjects[id] = obj;
        Attachements[obj] = new Dictionary<string, object>();
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
            Attachements[obj] = new Dictionary<string, object>();
            return id;
        }
    }

    public void addAttachement(object obj, string name, object value)
    {
        if (!Attachements.TryGetValue(obj, out var attachments))
        {
            attachments = new Dictionary<string, object>();
            Attachements[obj] = attachments;
        }
        attachments[name] = value;
    }

    internal void setAttachements(object obj, Dictionary<string, object> value)
    {
        Attachements[obj] = value;
    }

    public void removeAttachement(object obj, string name)
    {
        Attachements[obj].Remove(name);
    }

    public Dictionary<string, object>? getAttachements(object obj)
    {
        if (SubObjectIds.TryGetValue(obj, out var attachments))
        {
            return Attachements[obj];
        }
        return null;
    }
}