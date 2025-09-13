public class Field
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public Func<object?> get { get; set; }
    public Action<object?> set { get; set; }

    public Field(string name, Type type, Func<object?> get, Action<object?> set)
    {
        Name = name;
        Type = type;
        this.get = get;
        this.set = set;
    }
}