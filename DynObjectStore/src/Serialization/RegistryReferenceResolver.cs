namespace DynObjectStore;

using Newtonsoft.Json.Serialization;

public class RegistryReferenceResolver(ObjectReferences objRef) : IReferenceResolver
{
    // "#XY" declares a reference as attachment
    public bool IsReferenced(object context, object value) // check if already defined
    {
        return objRef.getSubObjectId(value) != null;
    }

    public string GetReference(object context, object value) // find or create new id
    {
        return objRef.registerSubObject(value).ToString();
    }

    public object ResolveReference(object context, string reference) // find by id
    {
        int index = int.Parse(reference);
        return objRef.getSubObject(index) ?? throw new Exception($"Object with ID {index} not found.");
    }

    public void AddReference(object context, string reference, object value)
    {
        int id = int.Parse(reference);
        objRef.registerSubObject(value, id);
    }
}
