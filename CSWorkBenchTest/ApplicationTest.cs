namespace CSWorkBenchTest;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        // setup
        Assembly assembly = Assembly.LoadFrom(@"..\..\..\..\TestClasses\bin\Debug\net8.0\TestClasses.dll");

        Type? personType = assembly.GetType("Person");
        Assert.NotNull(personType);
        object? person = Activator.CreateInstance(personType);
        Assert.NotNull(person);
        personType.GetProperty("Name")!.SetValue(person, "John");
        personType.GetProperty("Age")!.SetValue(person, 30);

        Type? childType = assembly.GetType("Child");
        Assert.NotNull(childType);
        object? child = Activator.CreateInstance(childType);
        Assert.NotNull(child);
        childType.GetProperty("Name")!.SetValue(child, "John");
        childType.GetProperty("Age")!.SetValue(child, 30);
        childType.GetProperty("Parent")!.SetValue(child, person);
        var children = personType.GetProperty("Children")!.GetValue(person);
        children.GetType().GetMethod("Add")!.Invoke(children, new object[] { child });

        JsonSerializerOptions options = new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.Preserve };

        // testing
        Registry registry = connectToDB();
        Registry registry2 = connectToDB();

        int id = registry.SaveObject(person);

        object? person2 = registry2.GetObject(id);
        Assert.NotNull(person2);
        Assert.IsType(personType, person2);

        Assert.Equal(JsonSerializer.Serialize(person, options), JsonSerializer.Serialize(person, options));

        registry.DeleteObject(id);
        Registry registry3 = connectToDB();
        object? person3 = registry3.GetObject(id);
        Assert.Null(person3);
        //TODO test changing paths

        //TODO test attachments
    }
}