# Getting started
- (Optional) Create a new project in your repository
- Create a new folder containing your migration SQL files
- Execute the following

```
KDG.Migrations.Migrations migrations = new Migrations(
    new MigrationConfig(
        'database-connection-string',
        'path-to-migrations-folder'
    )
)
await migrations.Migrate()
```