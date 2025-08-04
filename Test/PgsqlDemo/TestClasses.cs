class Person
{
    private string Name;
    private int Age;
    public List<Child> children = new List<Child>();

    public Person(string name, int age)
    {
        this.Name = name;
        this.Age = age;
    }
}

class Child : Person
{
    public Person Parent;
    public string? FavoriteToy;

    public Child(string name, int age, Person parent) : base(name, age)
    {
        this.Parent = parent;
    }
}