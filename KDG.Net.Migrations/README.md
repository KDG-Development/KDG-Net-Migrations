# KDG.Net.Migrations

A simple wrapper around DbUp for managing database migrations with support for PostgreSQL and SQL Server.

## Supported Databases

- PostgreSQL
- SQL Server

## Getting Started

1. (Optional) Create a new project in your repository
2. Create a new folder containing your migration SQL files
3. Execute the following code:

### PostgreSQL Example

```csharp
using KDG.Migrations;

var config = new MigrationConfig(
    KDG.Migrations.DatabaseType.PostgreSQL,
    "Host=localhost;Database=mydb;Username=user;Password=pass",
    "path-to-migrations-folder"
);

var migrations = new Migrations(config);
int result = migrations.Migrate();
```

### SQL Server Example

```csharp
using KDG.Migrations;

var config = new MigrationConfig(
    KDG.Migrations.DatabaseType.SqlServer,
    "Server=localhost;Database=mydb;User Id=user;Password=pass;",
    "path-to-migrations-folder"
);

var migrations = new Migrations(config);
int result = migrations.Migrate();
```

## Migration Files

- Create SQL migration files in your migrations folder
- Files are executed in alphabetical order (naming convention: `001_Description.sql`, `002_NextMigration.sql`, etc.)
- Subdirectories are automatically scanned and included
- All migrations run within a transaction and will rollback on failure

## Return Values

- `0` - Success
- `-1` - Migration failed

## Support

For support, please open an issue on our [GitHub Issues page](https://github.com/KDG-Development/KDG-Net-Migrations/issues) and provide your questions or feedback. We strive to address all inquiries promptly.

## Contributing

To contribute to this project, please follow these steps:

1. Fork the repository to your own GitHub account.
2. Make your changes and commit them to your fork.
3. Submit a pull request to the original repository with a clear description of what your changes do and why they should be included.
