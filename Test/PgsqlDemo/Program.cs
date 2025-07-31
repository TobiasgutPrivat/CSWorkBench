const string ConnString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=CSWorkBenchDB";

DBConnection connection = new DBConnection(ConnString);
Registry registry = new Registry(connection);

Person person = new Person("John", 30);
Child child = new Child("Andrew", 10, person);
person.children.Add(child); // for recursion


// 1. serialize Objects
registry.SaveObject(person);
// 2. recreate Objects
Registry registry2 = new Registry(connection); //simulates restart
Person recoveredPerson = (Person)registry2.GetObject(registry.ObjectIds[person]);
Console.WriteLine(recoveredPerson.Name);
Console.WriteLine(recoveredPerson.children[0].Name);
// 3. store Attachements
Person person2 = new Person("John", 30); // for attaching
registry.CreateAttachment(person, "", "friend1", person2);
// 4. read Attachements
List<AttachmentId> attachments = registry2.GetAttachmentIds(recoveredPerson);
Console.WriteLine(attachments[0].Item3);
// 5. delete Objects/Attachements
registry.DeleteObject(person);

connection.Close();