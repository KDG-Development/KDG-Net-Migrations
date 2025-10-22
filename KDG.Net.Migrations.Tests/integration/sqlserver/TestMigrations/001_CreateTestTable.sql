-- Create a test table for migration testing
CREATE TABLE test_table (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    created_at DATETIME DEFAULT GETDATE()
);



