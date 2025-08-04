const string ConnString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=CSWorkBenchDB";

DBConnection connection = new DBConnection(ConnString);
Registry registry = new Registry(connection);

Person person = new Person() { Name = "John", Age = 30 };
Child child = new Child() { Name = "Andrew", Age = 10, Parent = person, FavoriteToy = null };
person.children.Add(child); // for recursion


// 1. serialize Objects
registry.SaveObject(person);
registry.SaveObject(child);
// 2. recreate Objects
Registry registry2 = new Registry(connection); //simulates restart
Person recoveredPerson = (Person)registry2.GetObject(registry.ObjectIds[person]);
Person recoveredChild = (Person)registry2.GetObject(registry.ObjectIds[child]);
// 3. store Attachements
// Person person2 = new Person("John", 30); // for attaching
// registry.CreateAttachment(person, "", "friend1", person2);
// 4. read Attachements
List<AttachmentId> attachments = registry2.GetAttachmentIds(recoveredPerson);
Console.WriteLine(attachments[0].Item3);
// 5. delete Objects/Attachements
registry.DeleteObject(person);

connection.Close();