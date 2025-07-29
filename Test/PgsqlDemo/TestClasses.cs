class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public List<Child> children { get; set; } = new List<Child>();

    public Person(string name, int age)
    {
        this.Name = name;
        this.Age = age;
    }
}

class Child : Person
{
    public Person Parent { get; set; }
    public string? FavoriteToy { get; set; }

    public Child(string name, int age, Person parent) : base(name, age)
    {
        this.Parent = parent;
    }
}