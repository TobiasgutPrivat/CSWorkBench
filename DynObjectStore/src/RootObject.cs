namespace DynObjectStore;

public class RootObject
{
    // represents a full object graph based on one root object
    // stores id's of objects during deserialization to use same id's for serialization (allows more consistent identification)
    // also manages attachments

    public Registry registry = null!; // home registry
    public object root = null!;
    public int id;
    private readonly Dictionary<object, int> subObjectIds = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<int, object> subObjects = [];
    private readonly Dictionary<object, Dictionary<string, RootObject>> attachements = new(ReferenceEqualityComparer.Instance);
    internal int nextId = 0;

    public void AddSubObject(object obj, int nextId)
    {
        subObjectIds[obj] = nextId;
        subObjects[nextId] = obj;
    }

    public object? GetSubObject(int id)
    {
        if (subObjects.TryGetValue(id, out var obj))
        {
            return obj;
        }
        return null;
    }

    public int? GetSubObjectId(object obj)
    {
        if (subObjectIds.TryGetValue(obj, out var id))
        {
            return id;
        }
        return null;
    }

    public void AddAttachement(object obj, string name, RootObject attachement)
    {
        if (!attachements.TryGetValue(obj, out var attachments))
        {
            attachments = new Dictionary<string, RootObject>();
            attachements[obj] = attachments;
        }
        attachments[name] = attachement;
    }
    public async Task AddAttachement(object obj, string name, object attachement)
    {
        AddAttachement(obj, name, await registry.SaveObject(attachement));
    }

    internal void SetAttachements(object obj, Dictionary<string, RootObject>? value)
    {
        if (value == null)
        {
            attachements.Remove(obj);
            return;
        }
        attachements[obj] = value;
    }

    public void RemoveAttachement(object obj, string name)
    {
        if (attachements.TryGetValue(obj, out var attachments))
        {
            attachments.Remove(name);
        }
    }

    public Dictionary<string, RootObject>? GetAttachements(object obj)
    {
        if (attachements.TryGetValue(obj, out var attachments))
        {
            return attachments;
        }
        return null;
    }

    // internal for serialization
    internal int RegisterSubObject(object obj, int id)
    {
        if (GetSubObject(id) != null)
        {
            throw new Exception($"Object with ID {id} already exists.");
        }
        nextId = Math.Max(nextId, id + 1);
        subObjectIds[obj] = id;
        subObjects[id] = obj;
        return id;
    }

    internal int RegisterSubObject(object obj)
    {
        if (subObjectIds.TryGetValue(obj, out var id))
        {
            return id;
        }
        else
        {
            id = nextId;
            nextId++;
            subObjectIds[obj] = id;
            subObjects[id] = obj;
            attachements[obj] = new Dictionary<string, RootObject>();
            return id;
        }
    }
}