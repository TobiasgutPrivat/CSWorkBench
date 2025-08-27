namespace DynObjectStore;

using Npgsql;

public class PgDBConnection : IDBConnection
{
    private readonly NpgsqlConnection connection;

    public PgDBConnection(string connString)
    {
        this.connection = new NpgsqlConnection(connString);
    }

    public async Task Open()
    {
        await connection.OpenAsync();
        var cmd = new NpgsqlCommand(
            "CREATE TABLE IF NOT EXISTS \"Object\" (\n" +
            "    \"id\" SERIAL PRIMARY KEY,\n" +
            "    \"class\" VARCHAR(255) NOT NULL,\n" +
            "    \"data\" TEXT NOT NULL\n" +
            ");", connection);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task Dispose()
    {
        await connection.CloseAsync();
        await connection.DisposeAsync();
    }

    public async Task<int> CreateObject(string className, string data)
    {
        using var cmd = new NpgsqlCommand(
            "INSERT INTO \"Object\" (\"class\", \"data\") VALUES (@class, @data) RETURNING \"id\";",
            this.connection);

        cmd.Parameters.AddWithValue("class", className);
        cmd.Parameters.AddWithValue("data", data);

        var result = await cmd.ExecuteScalarAsync();
        return (int)result!;
    }

    public async Task<(string? typeName, string? jsonData)> GetObject(int objectId)
    {
        using var cmd = new NpgsqlCommand(
            "SELECT \"class\", \"data\" FROM \"Object\" WHERE \"id\" = @id;",
            this.connection);

        cmd.Parameters.AddWithValue("id", objectId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (reader.GetString(0), reader.GetString(1));
        }
        return (null, null);
    }

    public async Task UpdateObject(int objectId, string newData)
    {
        using var cmd = new NpgsqlCommand(
            "UPDATE \"Object\" SET \"data\" = @newData WHERE \"id\" = @id;",
            this.connection);

        cmd.Parameters.AddWithValue("newData", newData);
        cmd.Parameters.AddWithValue("id", objectId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteObject(int objectId)
    {
        using var cmd = new NpgsqlCommand(
            "DELETE FROM \"Object\" WHERE \"id\" = @id;",
            this.connection);

        cmd.Parameters.AddWithValue("id", objectId);

        await cmd.ExecuteNonQueryAsync();
    }
}
