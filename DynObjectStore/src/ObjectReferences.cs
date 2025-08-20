public class ObjectReferences
{
    public Dictionary<object, int> SubObjectIds = [];
    public Dictionary<int, object> SubObjects = [];
    public Dictionary<object, Dictionary<string, object>> Attachements = [];
    public int nextId = 0;

}