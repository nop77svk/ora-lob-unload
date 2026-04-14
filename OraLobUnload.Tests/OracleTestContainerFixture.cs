namespace NoP77svk.OraLobUnload.Tests;

using System;
using System.Threading.Tasks;

using Oracle.ManagedDataAccess.Client;

using Testcontainers.Oracle;

using Xunit;

/// <summary>
/// Fixture that manages the lifecycle of an Oracle database test container.
/// </summary>
public class OracleTestContainerFixture : IAsyncLifetime
{
    private const string OracleContainerPassword = "Test1234!";
    private OracleContainer? _container;
    private OracleConnection? _connection;

    public int OracleContainerHostPort { get; init; } = 1522;

    public string? ConnectionString { get; private set; }

    public OracleConnection GetConnection()
    {
        if (_connection is null)
        {
            throw new InvalidOperationException("Fixture not initialized");
        }

        return _connection;
    }

    public async ValueTask InitializeAsync()
    {
        _container = new OracleBuilder("container-registry.oracle.com/database/free:latest-lite")
            .WithPassword(OracleContainerPassword)
            .WithAutoRemove(true)
            .WithCleanUp(true)
            .WithPortBinding(OracleContainerHostPort, 1521)
            .WithEnvironment(new Dictionary<string, string>()
            {
                ["ORACLE_PWD"] = OracleContainerPassword,
                ["ORACLE_CHARACTERSET"] = "al32utf8",
                ["ENABLE_ARCHIVELOG"] = "false",
                ["ENABLE_FORCE_LOGGING"] = "false"
            })
            .Build();

        await _container.StartAsync();

        ConnectionString = new OracleConnectionStringBuilder()
        {
            DataSource = $"127.0.0.1:{OracleContainerHostPort}/FREEPDB1",
            UserID = "SYSTEM",
            Password = OracleContainerPassword
        }.ConnectionString;

        _connection = new OracleConnection(ConnectionString);
        await _connection.OpenAsync();

        await InitializeTestSchema();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        if (_container is not null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }

    public async Task SeedTestDataAsync()
    {
        ArgumentNullException.ThrowIfNull(_connection);

        // Clear existing data
        using var deleteCommand = _connection.CreateCommand();
        deleteCommand.CommandText = "DELETE FROM test_lob_data";
        await deleteCommand.ExecuteNonQueryAsync();

        // Insert test data
        string insertSql = """
            INSERT INTO test_lob_data (id, file_name, blob_content, clob_content, bfile_content)
            VALUES (:id, :fileName, :blobContent, :clobContent, :bfileContent)
            """;

        using var command = _connection.CreateCommand();
        command.CommandText = insertSql;

        byte[] testBlobData = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]; // PNG header
        string testClobData = """
            This is a test CLOB containing multiple lines of text.
            Line 2: Lorem ipsum dolor sit amet.
            Line 3: Test complete.
            """;

        // Add test rows
        for (int i = 1; i <= 3; i++)
        {
            command.Parameters.Clear();
            command.Parameters.Add("id", i);
            command.Parameters.Add("fileName", OracleDbType.Varchar2, $"testfile_{i}", System.Data.ParameterDirection.Input);
            command.Parameters.Add("blobContent", OracleDbType.Blob, testBlobData, System.Data.ParameterDirection.Input);
            command.Parameters.Add("clobContent", OracleDbType.Clob, testClobData + $" Row {i}", System.Data.ParameterDirection.Input);
            command.Parameters.Add("bfileContent", OracleDbType.BFile, DBNull.Value, System.Data.ParameterDirection.Input);

            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task ClearTestDataAsync()
    {
        ArgumentNullException.ThrowIfNull(_connection);

        using var command = _connection.CreateCommand();
        command.CommandText = "DELETE FROM test_lob_data";
        await command.ExecuteNonQueryAsync();
    }

    private async Task InitializeTestSchema()
    {
        ArgumentNullException.ThrowIfNull(_connection);

        string createTableSql = """
            CREATE TABLE test_lob_data (
                id NUMBER PRIMARY KEY,
                file_name VARCHAR2(255),
                blob_content BLOB,
                clob_content CLOB,
                bfile_content BFILE,
                created_date TIMESTAMP DEFAULT SYSTIMESTAMP
            )
            """;

        using var command = _connection.CreateCommand();
        command.CommandText = createTableSql;
        await command.ExecuteNonQueryAsync();
    }
}
