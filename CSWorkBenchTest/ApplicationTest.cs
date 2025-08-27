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
        object? personObj = Activator.CreateInstance(personType);
        Assert.NotNull(personObj);
        personType.GetProperty("Name")!.SetValue(personObj, "John");
        personType.GetProperty("Age")!.SetValue(personObj, 30);

        Type childType = assembly.GetType("Child")!;
        object child = Activator.CreateInstance(childType)!;
        childType.GetProperty("Name")!.SetValue(child, "Alice");
        childType.GetProperty("Age")!.SetValue(child, 8);
        childType.GetProperty("Parent")!.SetValue(child, personObj);
        object children = personType.GetProperty("Children")!.GetValue(personObj)!;
        children.GetType().GetMethod("Add")!.Invoke(children, [child]);

        object child2 = Activator.CreateInstance(childType)!;
        childType.GetProperty("Name")!.SetValue(child2, "Bob");
        childType.GetProperty("Age")!.SetValue(child2, 12);
        childType.GetProperty("Parent")!.SetValue(child2, personObj);
        children.GetType().GetMethod("Add")!.Invoke(children, [child2]);

        JsonSerializerOptions options = new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.Preserve };

        // testing
        Registry registry = connectToDB();
        Registry registry2 = connectToDB();

        RootObject person = registry.SaveObject(personObj);

        RootObject? person2 = registry2.GetObject(person.id);
        Assert.NotNull(person2);
        Assert.IsType(personType, person2.root);

        Assert.Equal(JsonSerializer.Serialize(person, options), JsonSerializer.Serialize(person2, options));

        // test changing paths
        int subid = person.getSubObjectId(child2) ?? throw new Exception("SubId not found.");
        Assert.Equal(child2, person.getSubObject(subid));
        children.GetType().GetMethod("RemoveAt")!.Invoke(children, [0]);
        Assert.Equal(child2, person.getSubObject(subid));
        registry.SaveObject(person);
        Registry registry4 = connectToDB();
        object child2recover = registry4.GetObject(person.id)!.getSubObject(subid) ?? throw new Exception("Object not found.");
        Assert.Equal(child2.GetType().GetProperty("Name")!.GetValue(child2), child2recover.GetType().GetProperty("Name")!.GetValue(child2recover));

        // test attachments
        person.addAttachement(child, "image", new byte[] { 1, 2, 3, 4, 5 });
        person.addAttachement(child2, "image", new byte[] { 1, 2, 3, 4, 5 });
        registry.SaveObject(person);

        RootObject attachment = person.getAttachements(child2)!["image"];
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, attachment.root);
        Registry registry5 = connectToDB();
        RootObject person5 = registry5.GetObject(person.id)!;
        object child2rec = person5.getSubObject(subid)!;
        RootObject attachment2 = person5.getAttachements(child2rec)!["image"];
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, attachment2.root);

        // delete
        registry.DeleteObject(person.id);
        Registry registry3 = connectToDB();
        object? person3 = registry3.GetObject(person.id);
        Assert.Null(person3);
    }
}