namespace DynObjectStore;

using Newtonsoft.Json;

public class Registry(IDBConnection db)
{
    // like local memory connected to drive (here database-service)

    internal Dictionary<int, object> Objects = [];
    internal Dictionary<object, int> ObjectIds = [];
    internal Dictionary<object, ObjectReferences> ObjectReferences = []; // rootobject -> subobject -> id (created from serialization)

    private static readonly JsonSerializerSettings options = new JsonSerializerSettings
    {
        PreserveReferencesHandling = PreserveReferencesHandling.All,
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        // TypeNameHandling = TypeNameHandling.All, // not really needed, might be safer in some cases
        ContractResolver = new FullAccessContractResolver(),
        Formatting = Formatting.Indented, // not needed, but nice for formatting
    };

    public void SaveObject(int id)
    {
        if (Objects.TryGetValue(id, out object? obj))
        {
            // options.resolver = ;
            string jsonData = JsonConvert.SerializeObject(obj, options);
            db.UpdateObject(id, jsonData);
        }
    }

    public int SaveObject(object obj)
    {
        options.ReferenceResolverProvider = () => new RegistryReferenceResolver(this, ObjectReferences[obj]);
        options.Converters = [new AttachmentAwareConverter<object>(this, ObjectReferences[obj])];
        string jsonData = JsonConvert.SerializeObject(obj, options);
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
            ObjectReferences[obj] = new ObjectReferences();
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

    public object? GetObject(int id)
    {
        if (Objects.TryGetValue(id, out object? value)) return value;

        db.GetObject(id, out string? className, out string? data);
        if (className == null || data == null) return null;

        Type type = Type.GetType(className) ?? throw new Exception($"Type {className} not found.");

        object obj = JsonConvert.DeserializeObject(data, type, options) ?? throw new Exception($"Object with ID {id} not found.");
        Objects[id] = obj;
        ObjectIds[obj] = id;
        return obj;
    }

    public void SetAttachment(object parent, object subObject, string name, object obj)
    {
        SaveObject(parent);

        if (!ObjectReferences[parent].Attachements.TryGetValue(subObject, out var attachments))
        {
            attachments = new Dictionary<string, object>();
            ObjectReferences[parent].Attachements[subObject] = attachments;
        }
        attachments[name] = subObject;

        SaveObject(obj);
    }

    public void DeleteAttachment(object parent, object subObject, string name)
    {
        ObjectReferences[parent].Attachements[subObject].Remove(name);
        SaveObject(parent);
    }

    public List<Tuple<string, string, object>> GetAttachments(object parent, object subObject)
    {
        if (!ObjectReferences[parent].Attachements.TryGetValue(subObject, out var attachments))
        {
            attachments = new Dictionary<string, object>();
            ObjectReferences[parent].Attachements[subObject] = attachments;
        }
        return attachments.Select(x => new Tuple<string, string, object>(x.Key, x.Key, x.Value)).ToList();

    }
}