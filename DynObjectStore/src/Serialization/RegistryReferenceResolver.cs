namespace DynObjectStore;

using Newtonsoft.Json.Serialization;

public class RegistryReferenceResolver(Registry registry, ObjectReferences objRef) : IReferenceResolver
{
    // "#XY" declares a reference as attachment
    public bool IsReferenced(object context, object value) // check if already defined
    {
        return objRef.SubObjectIds.TryGetValue(value, out var index);
    }

    public string GetReference(object context, object value) // find or create new id
    {
        if (!objRef.SubObjectIds.TryGetValue(value, out int id))
        {
            id = objRef.nextId; // or fetch from DB if persisted
            objRef.nextId++;
            objRef.SubObjectIds[value] = id;
            objRef.SubObjects[id] = value;
        }

        return id.ToString();
    }

    public object ResolveReference(object context, string reference) // find by id
    {
        int index = int.Parse(reference);
        if (objRef.SubObjects.TryGetValue(index, out var value))
        {
            return value;
        }
        return null;
    }

    public void AddReference(object context, string reference, object value)
    {
        int id = int.Parse(reference);
        if (objRef.SubObjects.TryGetValue(id, out var obj))
        {
            throw new Exception($"Object with ID {id} already exists.");
        }
        objRef.SubObjectIds[value] = id;
        objRef.SubObjects[id] = value;
        objRef.nextId = Math.Max(objRef.nextId, id + 1);
    }
}
