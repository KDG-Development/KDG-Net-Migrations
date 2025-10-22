namespace KDG.Migrations;

using DbUp.ScriptProviders;

public enum DatabaseType
{
    PostgreSQL,
    SqlServer
}

public class MigrationConfig
{
    public DatabaseType DatabaseType { get; set; }
    public string ConnectionString { get; set; }
    public string PathToMigrationsFolder { get; set; }

    public MigrationConfig(DatabaseType databaseType, string connectionString, string pathToMigrationsFolder)
    {
        DatabaseType = databaseType;
        ConnectionString = connectionString;
        PathToMigrationsFolder = pathToMigrationsFolder;
    }
}

public class Migrations
{
    private MigrationConfig _config { get; set; }
    
    public Migrations(
        MigrationConfig config
    ){
        this._config = config;
    }

    public int Migrate(){
        var fsso = new FileSystemScriptOptions();
        fsso.IncludeSubDirectories = true;

        var upgrader = this._config.DatabaseType switch
        {
            DatabaseType.PostgreSQL => DbUp.DeployChanges.To
                .PostgresqlDatabase(this._config.ConnectionString)
                .WithTransaction()
                .LogToConsole()
                .WithScriptsFromFileSystem(this._config.PathToMigrationsFolder, fsso)
                .Build(),
            
            DatabaseType.SqlServer => DbUp.DeployChanges.To
                .SqlDatabase(this._config.ConnectionString)
                .WithTransaction()
                .LogToConsole()
                .WithScriptsFromFileSystem(this._config.PathToMigrationsFolder, fsso)
                .Build(),
            
            _ => throw new ArgumentException($"Unsupported database type: {this._config.DatabaseType}")
        };

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(result.Error);
            Console.ResetColor();
            #if DEBUG
                Console.ReadLine();
            #endif
            return -1;
        }

        return 0;
    }
}