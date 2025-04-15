using System.Text.Json;

class Registry(DBConnection db) {
    //local Memory storage connected to one database.
    Dictionary<int, object> Objects = [];
    Dictionary<object, int> ObjectIds = [];
    Dictionary<object, List<Attachement>> Attachements = [];

    public List<object> GetRootObjects() {
        List<int> rootIds = db.GetRootObjects();
        return rootIds.Select(id => GetObject(id)).ToList();
    }

    public void SetRootObject(object obj) {
        if (ObjectIds.TryGetValue(obj, out int id)) {
            db.SetRootObject(id);
        } else {
            throw new Exception("Object not found in registry.");
        }
    }

    public void SaveObject(object obj) {
        string jsonData = JsonSerializer.Serialize(obj);
        if (ObjectIds.TryGetValue(obj, out int id)) {
            db.UpdateObject(id, jsonData);
        } else {
            int newId = db.CreateObject(obj.GetType().Name, jsonData);
            Objects[newId] = obj;
            ObjectIds[obj] = newId;
        }
    }

    public void CreateAttachment(object parent, string path, string name, object obj) {
        if (ObjectIds.TryGetValue(parent, out int parentId) && ObjectIds.TryGetValue(obj, out int objId)) {
            db.CreateAttachment(parentId, path, name, objId);
            if (!Attachements.ContainsKey(parent)) {
                Attachements[parent] = [];
            }
            Attachements[parent].Add(new Attachement(path, name, obj));
        } else {
            throw new Exception("Parent object not found in registry.");
        }
    }

    public void DeleteAttachment(object parent, string path, string name) {
        if (Attachements.TryGetValue(parent, out List<Attachement> attachments)) {
            attachments.ForEach(a => {if (a.Path == path && a.Name == name) {
                db.DeleteAttachment(ObjectIds[parent], path, name);
                attachments.Remove(a);
            }});
        } else {
            throw new Exception("Parent object not found in registry.");
        }
    }

    private object GetObject(int id) {
        if (Objects.TryGetValue(id, out object? value)) {
            return value;
        } else {
            db.GetObject(id, out string className, out string data);
            Type type = Type.GetType(className) ?? throw new Exception($"Type {className} not found.");
            object obj = JsonSerializer.Deserialize(data, type) ?? throw new Exception($"Object with ID {id} not found.");
            Objects[id] = obj;
            ObjectIds[obj] = id;
            return obj;
        }
    }

    public List<Attachement> GetAttachments(object parent) {
        if (Attachements.TryGetValue(parent, out List<Attachement> attachments)) {
            return attachments;
        } else {
            throw new Exception("Parent object not found in registry.");
        }
    }
}