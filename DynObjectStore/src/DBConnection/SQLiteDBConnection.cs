using Microsoft.Data.Sqlite;

namespace DynObjectStore
{
    public class SQLiteDBConnection : IDBConnection
    {
        private readonly string _dbPath;
        private SqliteConnection? _connection;

        public SQLiteDBConnection(string dbPath)
        {
            _dbPath = dbPath;
        }

        public async Task Open()
        {
            bool createNew = !File.Exists(_dbPath);

            _connection = new SqliteConnection($"Data Source={_dbPath}");
            _connection.Open();

            if (createNew)
            {
                using var cmd = _connection.CreateCommand();
                cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Objects (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TypeName TEXT NOT NULL,
                    JsonData TEXT NOT NULL
                );";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<int> CreateObject(string typeName, string jsonData)
        {
            if (_connection == null) throw new InvalidOperationException("Connection not opened");

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Objects (TypeName, JsonData) VALUES (@typeName, @jsonData); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@typeName", typeName);
            cmd.Parameters.AddWithValue("@jsonData", jsonData);

            var id = (long)await cmd.ExecuteScalarAsync();
            return (int)id;
        }

        public async Task UpdateObject(int id, string jsonData)
        {
            if (_connection == null) throw new InvalidOperationException("Connection not opened");

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "UPDATE Objects SET JsonData=@jsonData WHERE Id=@id;";
            cmd.Parameters.AddWithValue("@jsonData", jsonData);
            cmd.Parameters.AddWithValue("@id", id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteObject(int id)
        {
            if (_connection == null) throw new InvalidOperationException("Connection not opened");

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Objects WHERE Id=@id;";
            cmd.Parameters.AddWithValue("@id", id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<(string? typeName, string? jsonData)> GetObject(int id)
        {
            if (_connection == null) throw new InvalidOperationException("Connection not opened");

            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT TypeName, JsonData FROM Objects WHERE Id=@id;";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return (reader.GetString(0), reader.GetString(1));
            }

            return (null, null);
        }

        public Task Dispose()
        {
            _connection?.Dispose();
            _connection = null;
            return Task.CompletedTask;
        }
    }
}
