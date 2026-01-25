using Microsoft.Data.Sqlite;

namespace EnananBot.Connector;

/// <summary>
/// Manages SQLite database connections with performance optimizations.
/// </summary>
public sealed class SqLiteConn
{
    private readonly string _connectionString;

    public SqLiteConn(string dbName = "data/EnananBot.db")
    {
        // Ensure a directory exists (important for Docker volumes)
        var dbDirectory = Path.GetDirectoryName(dbName);
        if (!string.IsNullOrEmpty(dbDirectory))
            Directory.CreateDirectory(dbDirectory);

        _connectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource = dbName,

                // Enable connection pooling to reuse connections (improves performance)
                Pooling = true,

                // Automatically create the DB file if it doesn't exist
                Mode = SqliteOpenMode.ReadWriteCreate,

                // Timeout in seconds before throwing an error if the DB is locked
                DefaultTimeout = 5,

                // Enable shared caching for better memory usage across connections
                Cache = SqliteCacheMode.Shared
            }.ToString();
    }

    /// <summary>
    /// Creates and opens a new SQLite connection. 
    /// NOTE: The caller is responsible for Disposing the connection (using/await using).
    /// </summary>
    public SqliteConnection GetConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();

        InitializeConnection(conn);

        return conn;
    }

    /// <summary>
    /// Configures SQLite operational parameters (Pragmas) for high performance.
    /// </summary>
    private static void InitializeConnection(SqliteConnection conn)
    {
        using var cmd = conn.CreateCommand();

        cmd.CommandText = """
            PRAGMA journal_mode = WAL;
            PRAGMA synchronous = NORMAL;
            PRAGMA temp_store = MEMORY;
            PRAGMA foreign_keys = ON;
            PRAGMA cache_size = -200000;
            """;

        cmd.ExecuteNonQuery();
    }
}