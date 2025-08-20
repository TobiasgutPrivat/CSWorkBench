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

        Type childType = assembly.GetType("Child")!;
        object child = Activator.CreateInstance(childType)!;
        childType.GetProperty("Name")!.SetValue(child, "Alice");
        childType.GetProperty("Age")!.SetValue(child, 8);
        childType.GetProperty("Parent")!.SetValue(child, person);
        object children = personType.GetProperty("Children")!.GetValue(person)!;
        children.GetType().GetMethod("Add")!.Invoke(children, [child]);

        object child2 = Activator.CreateInstance(childType)!;
        childType.GetProperty("Name")!.SetValue(child2, "Bob");
        childType.GetProperty("Age")!.SetValue(child2, 12);
        childType.GetProperty("Parent")!.SetValue(child2, person);
        children.GetType().GetMethod("Add")!.Invoke(children, [child2]);

        JsonSerializerOptions options = new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.Preserve };

        // testing
        Registry registry = connectToDB();
        Registry registry2 = connectToDB();

        int id = registry.SaveObject(person);

        object? person2 = registry2.GetObject(id);
        Assert.NotNull(person2);
        Assert.IsType(personType, person2);

        Assert.Equal(JsonSerializer.Serialize(person, options), JsonSerializer.Serialize(person, options));

        // test changing paths
        int subid = registry.GetSubId(person, child2) ?? throw new Exception("SubId not found.");
        Assert.Equal(child2, registry.GetObject(id, subid));
        children.GetType().GetMethod("RemoveAt")!.Invoke(children, [0]);
        Assert.Equal(child2, registry.GetObject(id, subid));
        Registry registry4 = connectToDB();
        object child2recover = registry4.GetObject(id, subid)!;
        Assert.Equal(child2.GetType().GetProperty("Name")!.GetValue(child2), child2recover.GetType().GetProperty("Name")!.GetValue(child2recover));

        // test attachments
        registry.SetAttachment(person, child, "image", new byte[] { 1, 2, 3, 4, 5 });
        registry.SetAttachment(person, child2, "image", new byte[] { 1, 2, 3, 4, 5 });

        object attachment = registry.GetAttachements(person, child)!["image"];
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, attachment);
        Registry registry5 = connectToDB();
        attachment = registry5.GetAttachements(person, child)!["image"];
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, attachment);

        // delete
        registry.DeleteObject(id);
        Registry registry3 = connectToDB();
        object? person3 = registry3.GetObject(id);
        Assert.Null(person3);
    }
}