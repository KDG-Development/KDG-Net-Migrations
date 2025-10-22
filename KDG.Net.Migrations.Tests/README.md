# KDG.Net.Migrations Tests

This project contains unit and integration tests for the KDG.Net.Migrations library.

## Project Structure

```
KDG.Net.Migrations.Tests/
├── unit/
│   └── postgres/          # Unit tests for PostgreSQL (placeholder for future tests)
└── integration/
    └── postgres/          # PostgreSQL integration tests
        ├── TestMigrations/    # Sample SQL files for testing
        │   ├── 001_CreateTestTable.sql
        │   ├── 002_InsertData.sql
        │   ├── SubFolder/
        │   │   └── 003_AlterTable.sql
        │   └── 999_InvalidSql.sql
        └── PostgresMigrationTests.cs
```

## Prerequisites

- .NET 8.0 SDK
- Docker Desktop (for Testcontainers to run PostgreSQL containers)

## Running Tests

### Restore Dependencies

```bash
dotnet restore
```

### Run All Tests

```bash
dotnet test
```

### Run Specific Test

```bash
dotnet test --filter "FullyQualifiedName~PostgresMigrationTests.SuccessfulMigration_ReturnsZero"
```

## Test Coverage

### Integration Tests (PostgreSQL)

- **SuccessfulMigration_ReturnsZero**: Verifies that valid migrations execute successfully and return status code 0
- **FailedMigration_ReturnsNegativeOne**: Tests that invalid SQL causes migration to fail and return status code -1
- **TransactionRollback_OnFailure**: Confirms that failed migrations trigger a transaction rollback, leaving the database unchanged
- **SubdirectoryScanning_IncludesNestedScripts**: Validates that migration scripts in subdirectories are discovered and executed

## Adding New Tests

### For New Database Engines

When adding support for a new database engine (e.g., MySQL, SQL Server):

1. Create a new folder under `unit/[engine-name]` for unit tests
2. Create a new folder under `integration/[engine-name]` for integration tests
3. Add test migration SQL files in `integration/[engine-name]/TestMigrations/`
4. Create test class similar to `PostgresMigrationTests.cs`
5. Add appropriate Testcontainers package for the database engine

### Test Naming Convention

- Test classes: `[DatabaseEngine]MigrationTests.cs`
- Test methods: `[Scenario]_[ExpectedResult]`

## Notes

- Integration tests use Testcontainers to spin up isolated PostgreSQL containers
- Each test method gets its own database container to ensure isolation
- Containers are automatically cleaned up after tests complete
- Test migration files are copied to the output directory during build

