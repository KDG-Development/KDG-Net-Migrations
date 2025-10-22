namespace KDG.Migrations.Tests.Integration.Postgres;

using Testcontainers.PostgreSql;
using Xunit;
using KDG.Migrations;

public class PostgresMigrationTests : IAsyncLifetime
{
    private PostgreSqlContainer _container = null!;
    private string _connectionString = null!;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();

        await _container.StartAsync();
        _connectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    [Fact]
    public void SuccessfulMigration_ReturnsZero()
    {
        // Arrange
        var migrationsPath = Path.Combine(AppContext.BaseDirectory, "integration", "postgres", "TestMigrations");
        
        // Only use valid migrations (exclude 999_InvalidSql.sql)
        var validMigrationsPath = Path.Combine(migrationsPath, "valid");
        Directory.CreateDirectory(validMigrationsPath);
        
        // Copy valid migrations to temp location
        File.Copy(
            Path.Combine(migrationsPath, "001_CreateTestTable.sql"),
            Path.Combine(validMigrationsPath, "001_CreateTestTable.sql"),
            true
        );
        File.Copy(
            Path.Combine(migrationsPath, "002_InsertData.sql"),
            Path.Combine(validMigrationsPath, "002_InsertData.sql"),
            true
        );
        
        // Copy subdirectory
        var subFolder = Path.Combine(validMigrationsPath, "SubFolder");
        Directory.CreateDirectory(subFolder);
        File.Copy(
            Path.Combine(migrationsPath, "SubFolder", "003_AlterTable.sql"),
            Path.Combine(subFolder, "003_AlterTable.sql"),
            true
        );

        var config = new MigrationConfig(DatabaseType.PostgreSQL, _connectionString, validMigrationsPath);
        var migrations = new Migrations(config);

        // Act
        var result = migrations.Migrate();

        // Assert
        Assert.Equal(0, result);
        
        // Cleanup
        Directory.Delete(validMigrationsPath, true);
    }

    [Fact]
    public void FailedMigration_ReturnsNegativeOne()
    {
        // Arrange
        var migrationsPath = Path.Combine(AppContext.BaseDirectory, "integration", "postgres", "TestMigrations");
        
        // Create temp directory with only invalid SQL
        var invalidMigrationsPath = Path.Combine(migrationsPath, "invalid");
        Directory.CreateDirectory(invalidMigrationsPath);
        
        File.Copy(
            Path.Combine(migrationsPath, "999_InvalidSql.sql"),
            Path.Combine(invalidMigrationsPath, "999_InvalidSql.sql"),
            true
        );

        var config = new MigrationConfig(DatabaseType.PostgreSQL, _connectionString, invalidMigrationsPath);
        var migrations = new Migrations(config);

        // Act
        var result = migrations.Migrate();

        // Assert
        Assert.Equal(-1, result);
        
        // Cleanup
        Directory.Delete(invalidMigrationsPath, true);
    }

    [Fact]
    public void TransactionRollback_OnFailure()
    {
        // Arrange
        var migrationsPath = Path.Combine(AppContext.BaseDirectory, "integration", "postgres", "TestMigrations");
        
        // Create temp directory with valid migration followed by invalid one
        var rollbackTestPath = Path.Combine(migrationsPath, "rollback");
        Directory.CreateDirectory(rollbackTestPath);
        
        File.Copy(
            Path.Combine(migrationsPath, "001_CreateTestTable.sql"),
            Path.Combine(rollbackTestPath, "001_CreateTestTable.sql"),
            true
        );
        File.Copy(
            Path.Combine(migrationsPath, "999_InvalidSql.sql"),
            Path.Combine(rollbackTestPath, "002_Invalid.sql"),
            true
        );

        var config = new MigrationConfig(DatabaseType.PostgreSQL, _connectionString, rollbackTestPath);
        var migrations = new Migrations(config);

        // Act
        var result = migrations.Migrate();

        // Assert
        Assert.Equal(-1, result);
        
        // Verify table was not created (transaction rolled back)
        using var connection = new Npgsql.NpgsqlConnection(_connectionString);
        connection.Open();
        
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'test_table')";
        var tableExists = (bool)cmd.ExecuteScalar()!;
        
        Assert.False(tableExists);
        
        // Cleanup
        Directory.Delete(rollbackTestPath, true);
    }

    [Fact]
    public void SubdirectoryScanning_IncludesNestedScripts()
    {
        // Arrange
        var migrationsPath = Path.Combine(AppContext.BaseDirectory, "integration", "postgres", "TestMigrations");
        
        // Use valid migrations path
        var validMigrationsPath = Path.Combine(migrationsPath, "subdir_test");
        Directory.CreateDirectory(validMigrationsPath);
        
        File.Copy(
            Path.Combine(migrationsPath, "001_CreateTestTable.sql"),
            Path.Combine(validMigrationsPath, "001_CreateTestTable.sql"),
            true
        );
        
        // Copy subdirectory migration
        var subFolder = Path.Combine(validMigrationsPath, "SubFolder");
        Directory.CreateDirectory(subFolder);
        File.Copy(
            Path.Combine(migrationsPath, "SubFolder", "003_AlterTable.sql"),
            Path.Combine(subFolder, "003_AlterTable.sql"),
            true
        );

        var config = new MigrationConfig(DatabaseType.PostgreSQL, _connectionString, validMigrationsPath);
        var migrations = new Migrations(config);

        // Act
        var result = migrations.Migrate();

        // Assert
        Assert.Equal(0, result);
        
        // Verify the column from subdirectory migration was added
        using var connection = new Npgsql.NpgsqlConnection(_connectionString);
        connection.Open();
        
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT EXISTS (
                SELECT FROM information_schema.columns 
                WHERE table_name = 'test_table' 
                AND column_name = 'description'
            )";
        var columnExists = (bool)cmd.ExecuteScalar()!;
        
        Assert.True(columnExists);
        
        // Cleanup
        Directory.Delete(validMigrationsPath, true);
    }
}
