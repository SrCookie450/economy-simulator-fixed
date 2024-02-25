using Dapper;
using Npgsql;

namespace Roblox.Services;

public static class Database
{
    private static string dbConnectionString { get; set; } = string.Empty;
    private static Mutex connectionMutex { get; } = new();

    public static void AcquireConnectionMutex(string debugReason)
    {
        connectionMutex.WaitOne();
    }

    public static void ReleaseConnectionMutex()
    {
        connectionMutex.ReleaseMutex();
    }

    public static NpgsqlConnection connection
    {
        get
        {
            AcquireConnectionMutex("Database.connection");
            var myConn = unsafeConnection;
            ReleaseConnectionMutex();
            return myConn;
        }
    }
    
    public static NpgsqlConnection unsafeConnection => new NpgsqlConnection(dbConnectionString);

    /// <summary>
    /// List of table names, use for preparing statements
    /// </summary>
    public static List<string> tableNames { get; set; } = new();

    /// <summary>
    /// A dictionary of tableName to tableColumns
    /// </summary>
    public static Dictionary<string, List<string>> tableToColumnMap { get; set; } = new();
    public static void Configure(string databaseConnectionString)
    {
        dbConnectionString = databaseConnectionString;

        var allTables = connection.Query("select * from information_schema.tables WHERE table_schema = :table_schema AND table_catalog = :table_catalog", new
        {
            table_schema = "public", // todo: this will be configurable
            table_catalog = connection.Database,
        });
        foreach (var item in allTables)
        {
            var cols = connection.Query("SELECT * FROM information_schema.columns WHERE table_schema = :schema AND table_name = :table AND table_catalog = :catalog", new
            {
                catalog = connection.Database,
                table = item.table_name,
                schema = "public",
            });
            tableNames.Add(item.table_name);
            tableToColumnMap.Add(item.table_name, new List<string>());
            foreach (var col in cols)
            {
                tableToColumnMap[item.table_name].Add(col.column_name);
            }
        }
        // Dapper config here
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    }
}
