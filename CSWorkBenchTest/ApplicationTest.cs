namespace CSWorkBenchTest;

using System.Reflection;
using DynObjectStore;
using Npgsql;

public class ApplicationTest
{
    public static Registry connectToDB()
    {
        var connBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = "localhost",
            Port = 5432,
            Username = "postgres",
            Password = "postgres",
            Database = "CSWorkBenchDB",
            SslMode = SslMode.Disable
        };
        IDBConnection connection = new PgDBConnection(connBuilder.ConnectionString);
        return new Registry(connection);
    }

    [Fact]
    public void Object()
    {
        Assembly assembly = Assembly.LoadFrom(@"..\..\..\..\TestClasses\bin\Debug\net8.0\TestClasses.dll");
        Registry registry = connectToDB();
        Registry registry2 = connectToDB();

        Type? personType = assembly.GetType("Person");
        Assert.NotNull(personType);
        object? person = Activator.CreateInstance(personType);
        Assert.NotNull(person);
        personType.GetProperty("Name")!.SetValue(person, "John");
        personType.GetProperty("Age")!.SetValue(person, 30);

        registry.SaveObject(person);
        int id = registry.ObjectIds[person];

        object? person2 = registry2.GetObject(id);
        Assert.NotNull(person2);
        Assert.IsType(personType, person2);
        Assert.Equal(person.ToString(), person2.ToString());

        registry.DeleteObject(id);
        Registry registry3 = connectToDB();
        object? person3 = registry3.GetObject(id);
        Assert.Null(person3);
    }
}