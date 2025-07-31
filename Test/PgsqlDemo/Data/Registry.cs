using Newtonsoft.Json;

class Registry(DBConnection db)
{
    //local Memory storage connected to one database.

    // in registry means in DB 1:1
    // delaying commmiting like in git could be added (runtime sessions)
    // user management could be added    

    Dictionary<int, object> Objects = [];
    public Dictionary<object, int> ObjectIds = [];
    Dictionary<AttachmentId, object> Attachements = []; // parent, path, name, object
    Dictionary<object, List<AttachmentId>> AttachementIds = [];

    JsonSerializerSettings options = new JsonSerializerSettings
    {
        PreserveReferencesHandling = PreserveReferencesHandling.All,
        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        Formatting = Formatting.Indented
    };

    public void SaveObject(object obj)
    {
        string jsonData = JsonConvert.SerializeObject(obj, options);
        if (ObjectIds.TryGetValue(obj, out int id))
        {
            db.UpdateObject(id, jsonData);
        }
        else
        {
            int newId = db.CreateObject(obj.GetType().Name, jsonData);
            Objects[newId] = obj;
            ObjectIds[obj] = newId;
        }
    }

    public object DeleteObject(object obj)
    {
        if (ObjectIds.TryGetValue(obj, out int id))
        {
            if (AttachementIds.TryGetValue(obj, out List<AttachmentId>? attachments))
            {
                attachments.ForEach(x => Attachements.Remove(x));
                AttachementIds.Remove(obj);
            }
            db.DeleteObject(id); // also deletes attachements in DB
            Objects.Remove(id);
            ObjectIds.Remove(obj);
            return obj;
        }
        else
        {
            throw new Exception("Object not stored in DB.");
        }
    }

    public object GetObject(int id)
    {
        if (Objects.TryGetValue(id, out object? value))
        {
            return value;
        }
        else
        {
            db.GetObject(id, out string className, out string data);
            Type type = Type.GetType(className) ?? throw new Exception($"Type {className} not found.");
            object obj = JsonConvert.DeserializeObject(data, type, options) ?? throw new Exception($"Object with ID {id} not found.");
            Objects[id] = obj;
            ObjectIds[obj] = id;
            return obj;
        }
    }

    public void CreateAttachment(object parent, string path, string name, object obj)
    {
        SaveObject(parent);
        List<AttachmentId> attachmentIds = GetAttachmentIds(parent);

        if (attachmentIds.Any(x => x.Item2 == path && x.Item3 == name))
        {
            throw new Exception("Attachment already exists.");
        }
        if (ObjectIds.TryGetValue(obj, out int id))
        {

        }
        SaveObject(obj);
        db.CreateAttachment(ObjectIds[parent], path, name, ObjectIds[obj]);
        AttachmentId attachmentId = new AttachmentId(parent, path, name);

        Attachements[attachmentId] = obj;
        attachmentIds.Add(attachmentId);
    }

    public void DeleteAttachment(object parent, string path, string name)
    {
        AttachmentId attachmentId = new AttachmentId(parent, path, name);
        Attachements.Remove(attachmentId);
        AttachementIds[parent].Remove(attachmentId);
        db.DeleteAttachment(ObjectIds[parent], path, name);
    }

    public List<Tuple<string, string, object>> GetAttachments(object parent)
    {
        return GetAttachmentIds(parent).Select(x => Tuple.Create(x.Item2, x.Item3, Attachements[x])).ToList();
    }

    public List<AttachmentId> GetAttachmentIds(object parent)
    {
        SaveObject(parent);
        if (!AttachementIds.TryGetValue(parent, out List<AttachmentId>? value)) // check if loaded from DB
        {
            value = [];
            int id = ObjectIds[parent];
            // load from DB
            List<Tuple<string, string, int>> attachments = db.GetAttachments(id);
            foreach (Tuple<string, string, int> attachment in attachments)
            {
                AttachmentId attachmentId = new AttachmentId(parent, attachment.Item1, attachment.Item2);
                Attachements[attachmentId] = GetObject(attachment.Item3);
                value.Add(attachmentId);
            }
            AttachementIds[parent] = value;
        }
        return value;
    }
}

class AttachmentId(object parent, string path, string name) : Tuple<object, string, string>(parent, path, name) { };