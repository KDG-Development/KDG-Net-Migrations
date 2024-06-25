namespace KDG.Migrations;

using DbUp.ScriptProviders;

public class MigrationConfig
{
    // TODO: ideally, this is a DU/tuple of DBKind,connectionstring
    // e.g. <PostgreSQL,'connection-string'>
    public string ConnectionString { get; set; }
    public string PathToMigrationsFolder { get; set; }

    public MigrationConfig(string connectionString, string pathToMigrationsFolder)
    {
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

        // TODO: this should be factored out to a DBEngine specific implementation
        // right now, this assumes postgresql
        var upgrader =
            DbUp.DeployChanges.To
            .PostgresqlDatabase(this._config.ConnectionString)
            .WithTransaction()
            .LogToConsole()
            .WithScriptsFromFileSystem(this._config.PathToMigrationsFolder,fsso)
            .Build();

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