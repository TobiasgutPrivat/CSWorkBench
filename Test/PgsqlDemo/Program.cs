const string ConnString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=CSWorkBenchDB";

DBConnection connection = new DBConnection(ConnString);
Registry registry = new Registry(connection);

Person person = new Person("John", 30);
Child child = new Child("Andrew", 10, person);
person.children.Add(child); // for recursion

Person person2 = new Person("John", 30); // for attaching

// 1. serialize Objects
registry.SaveObject(person);
// 2. recreate Objects
Registry registry2 = new Registry(connection);
Person recoveredPerson = (Person)registry2.GetObject(registry.GetObjectId(person));
Console.WriteLine(recoveredPerson.Name);
Console.WriteLine(recoveredPerson.children[0].Name);
// 3. store Attachements
// 4. read Attachements
// 5. delete Objects/Attachements
