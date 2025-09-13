using System.Reflection;

public class AttrInfo
{
    // designed to only provide public getters and setters
    public string Name { get; set; }
    public Type Type { get; set; }
    public Func<object?, object?> get { get; set; }
    public Action<object?, object?> set { get; set; }
    public bool CanRead { get; set; }
    public bool CanWrite { get; set; }

    public AttrInfo(FieldInfo fieldInfo)
    {
        Name = fieldInfo.Name;
        Type = fieldInfo.FieldType;
        get = fieldInfo.GetValue;
        set = fieldInfo.SetValue;
        CanRead = true;
        CanWrite = true;
    }

    public AttrInfo(PropertyInfo propertyInfo)
    {
        Name = propertyInfo.Name;
        Type = propertyInfo.PropertyType;
        get = propertyInfo.GetValue;
        set = propertyInfo.SetValue;
        CanRead = propertyInfo.GetGetMethod() != null;
        CanWrite = propertyInfo.GetSetMethod() != null;
    }
}