using Npgsql;

class DBConnection {
    // Handles all SQL operations

    private NpgsqlConnection connection;

    public DBConnection(string connString)
    {
        this.connection = new NpgsqlConnection(connString);
        this.connection.Open();
    }

    public void Close()
    {
        this.connection.Close();
    }

    public List<int> GetRootObjects() {
        var readCmd = new NpgsqlCommand(
            "SELECT \"id\" FROM \"Object\" WHERE \"root\";", this.connection);
        using var reader = readCmd.ExecuteReader();
        List<int> rootObjects = new List<int>();
        while (reader.Read())
        {
            rootObjects.Add(reader.GetInt32(0));
        }
        return rootObjects;
    }

    public void SetRootObject(int id) {
        var setRootCmd = new NpgsqlCommand(
            "UPDATE \"Object\" SET \"root\" = TRUE WHERE \"id\" = @id;", this.connection);
        setRootCmd.Parameters.AddWithValue("id", id);
        setRootCmd.ExecuteNonQuery();
    }

    public int CreateObject(string Class, string Data)
    {
        var createCmd = new NpgsqlCommand(
        "INSERT INTO \"Object\" (\"class\", \"data\") VALUES (@class, @data) RETURNING \"id\";", this.connection);
        createCmd.Parameters.AddWithValue("class", Class);
        createCmd.Parameters.AddWithValue("data", Data);
        var objectId = (int)createCmd.ExecuteScalar();
        return objectId;
    }
    public int CreateObject(int id, string Class, string Data)
    {
        var createCmd = new NpgsqlCommand(
        "INSERT INTO \"Object\" (\"class\", \"data\") VALUES (@id, @class, @data);", this.connection);
        createCmd.Parameters.AddWithValue("class", Class);
        createCmd.Parameters.AddWithValue("data", Data);
        createCmd.Parameters.AddWithValue("id", id);
        var objectId = (int)createCmd.ExecuteScalar();
        return objectId;
    }

    public void CreateAttachment(int parentId, string path, string name, int objectId)
    {
        var createAttachmentCmd = new NpgsqlCommand(
            "INSERT INTO \"Attachment\" (\"parent_id\", \"path\", \"name\", \"object_id\") VALUES (@parent_id, @path, @name, @object_id);", this.connection);
        createAttachmentCmd.Parameters.AddWithValue("parent_id", parentId);
        createAttachmentCmd.Parameters.AddWithValue("path", path);
        createAttachmentCmd.Parameters.AddWithValue("name", name);
        createAttachmentCmd.Parameters.AddWithValue("object_id", objectId);
        createAttachmentCmd.ExecuteNonQuery();
    }
    public void GetObject(int objectId, out string Class, out string data) //return (class, data)
    {
        var readCmd = new NpgsqlCommand(
            "SELECT \"class\", \"data\" FROM \"Object\" WHERE \"id\" = @id;", this.connection);
        readCmd.Parameters.AddWithValue("id", objectId);
        using var reader = readCmd.ExecuteReader();
        if (reader.Read()) {
            Class = reader.GetString(0);
            data = reader.GetString(1);
        } else {
            throw new Exception($"Object with ID {objectId} not found.");
        }
    }

    public List<Attachement> GetAttachments(int objectId) //return [(path, name, ObjecId),...]
    {
        var readCmd = new NpgsqlCommand(
            "SELECT \"path\", \"name\", \"object_id\" FROM \"Attachment\" WHERE \"parent_id\" = @id;", this.connection);
        readCmd.Parameters.AddWithValue("id", objectId);
        using var reader = readCmd.ExecuteReader();
        List<Attachement> attachments = [];
        while (reader.Read())
        {
            string path = reader.GetString(0);
            string name = reader.GetString(1);
            int objId = reader.GetInt32(2);
            attachments.Add(new Attachement(path, name, objId));
        }
        return attachments;
    }

    public void UpdateObject(int objectId, string newData)
    {
        var updateCmd = new NpgsqlCommand(
            "UPDATE \"Object\" SET \"data\" = @newData WHERE \"id\" = @id;", this.connection);
        updateCmd.Parameters.AddWithValue("newData", newData);
        updateCmd.Parameters.AddWithValue("id", objectId);
        updateCmd.ExecuteNonQuery();
    }

    public void DeleteObject(int objectId)
    {
        var deleteCmd = new NpgsqlCommand(
            "DELETE FROM \"Object\" WHERE \"id\" = @id;", this.connection);
        deleteCmd.Parameters.AddWithValue("id", objectId);
        deleteCmd.ExecuteNonQuery();
    }

    public void DeleteAttachment(int parentId, string path, string name)
    {
        var deleteCmd = new NpgsqlCommand(
            "DELETE FROM \"Attachment\" WHERE \"parent_id\" = @parentId AND \"path\" = @path AND \"name\" = @name;", this.connection);
        deleteCmd.Parameters.AddWithValue("parentId", parentId);
        deleteCmd.Parameters.AddWithValue("path", path);
        deleteCmd.Parameters.AddWithValue("name", name);
        deleteCmd.ExecuteNonQuery();
    }
}