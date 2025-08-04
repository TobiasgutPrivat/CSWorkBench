using Npgsql;

class DBConnection
{
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

    public int CreateObject(string Class, string Data)
    {
        var createCmd = new NpgsqlCommand(
        "INSERT INTO \"Object\" (\"class\", \"data\") VALUES (@class, @data) RETURNING \"id\";", this.connection);
        createCmd.Parameters.AddWithValue("class", Class);
        createCmd.Parameters.AddWithValue("data", Data);
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
        if (reader.Read())
        {
            Class = reader.GetString(0);
            data = reader.GetString(1);
        }
        else
        {
            throw new Exception($"Object with ID {objectId} not found.");
        }
    }

    public List<Tuple<string, string, int>> GetAttachments(int objectId) //return [(path, name, ObjecId),...]
    {
        var readCmd = new NpgsqlCommand(
            "SELECT \"path\", \"name\", \"object_id\" FROM \"Attachment\" WHERE \"parent_id\" = @id;", this.connection);
        readCmd.Parameters.AddWithValue("id", objectId);
        using var reader = readCmd.ExecuteReader();
        List<Tuple<string, string, int>> attachments = [];
        while (reader.Read())
        {
            string path = reader.GetString(0);
            string name = reader.GetString(1);
            int objId = reader.GetInt32(2);
            attachments.Add(new Tuple<string, string, int>(path, name, objId));
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