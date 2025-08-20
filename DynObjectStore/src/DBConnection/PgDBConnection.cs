namespace DynObjectStore;

using Npgsql;
public class PgDBConnection : IDBConnection
{
    // Handles all SQL operations

    private NpgsqlConnection connection;

    public PgDBConnection(string connString)
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
        var objectId = (int)createCmd.ExecuteScalar()!;
        return objectId;
    }

    public void GetObject(int objectId, out string? Class, out string? data) //return (class, data)
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
            Class = null;
            data = null;
        }
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
}