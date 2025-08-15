class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public List<Child> Children { get; set; } = new List<Child>();
}

class Child : Person
{
    public string FavoriteToy { get; set; }
    public Person Parent { get; set; }
}