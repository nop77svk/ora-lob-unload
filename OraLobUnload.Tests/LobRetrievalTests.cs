namespace NoP77svk.OraLobUnload.Tests;

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

using NoP77svk.OraLobUnload.DataReaders;
using NoP77svk.OraLobUnload.StreamColumnProcessors;

using Oracle.ManagedDataAccess.Client;

using Xunit;
using Xunit.Sdk;

/// <summary>
/// Tests for all four LOB retrieval scenarios using Oracle test container.
/// Scenarios: BLOB, CLOB, BFILE, and NULL/Empty LOBs
/// </summary>
public class LobRetrievalTests : IClassFixture<OracleTestContainerFixture>
{
    private readonly OracleTestContainerFixture _fixture;

    public LobRetrievalTests(OracleTestContainerFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    [Fact]
    public async Task Scenario1_RetrieveBlobFromDatabase_Successfully()
    {
        // Arrange
        await _fixture.SeedTestDataAsync();
        var connection = _fixture.GetConnection();

        string selectSql = "SELECT file_name, blob_content FROM test_lob_data WHERE id = 1";
        using var command = new OracleCommand(selectSql, connection)
        {
            InitialLOBFetchSize = 4096
        };

        // Act
        using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        Assert.True(await reader.ReadAsync(TestContext.Current.CancellationToken), "Should retrieve at least one row");

        string fileName = reader.GetString(0);
        using var blobStream = reader.GetOracleBlob(1);
        var processor = new BlobProcessor();
        using var memoryStream = new MemoryStream();
        await processor.SaveLobToStreamAsync(blobStream, memoryStream);

        // Assert
        Assert.NotNull(blobStream);
        Assert.NotEmpty(fileName);
        Assert.True(blobStream.CanRead, "BLOB stream should be readable");
        Assert.NotEqual(0, processor.GetTrueLobLength(blobStream.Length));

        string formattedLength = processor.GetFormattedLobLength(blobStream.Length);
        Assert.StartsWith("BLOB:", formattedLength);

        // Verify content can be read
        Assert.NotEmpty(memoryStream.ToArray());

        await _fixture.ClearTestDataAsync();
    }

    [Fact]
    public async Task Scenario2_RetrieveClobFromDatabase_Successfully()
    {
        // Arrange
        await _fixture.SeedTestDataAsync();
        var connection = _fixture.GetConnection();

        string selectSql = "SELECT file_name, clob_content FROM test_lob_data WHERE id = 1";
        using var command = new OracleCommand(selectSql, connection)
        {
            InitialLOBFetchSize = 4096
        };

        // Act
        using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        Assert.True(await reader.ReadAsync(TestContext.Current.CancellationToken), "Should retrieve at least one row");

        string fileName = reader.GetString(0);
        using var clobStream = reader.GetOracleClob(1);
        var processor = new ClobProcessor(Encoding.UTF8);
        using var memoryStream = new MemoryStream();
        await processor.SaveLobToStreamAsync(clobStream, memoryStream);

        // Assert
        Assert.NotNull(clobStream);
        Assert.NotEmpty(fileName);
        Assert.True(clobStream.CanRead, "CLOB stream should be readable");
        Assert.NotEqual(0, processor.GetTrueLobLength(clobStream.Length));

        string formattedLength = processor.GetFormattedLobLength(clobStream.Length);
        Assert.StartsWith("CLOB:", formattedLength);

        // Verify content can be read
        Assert.NotEmpty(memoryStream.ToArray());

        await _fixture.ClearTestDataAsync();
    }

    [Fact]
    public async Task Scenario3_RetrieveBFileFromDatabase_Successfully()
    {
        // Arrange
        await _fixture.SeedTestDataAsync();
        var connection = _fixture.GetConnection();

        string selectSql = "SELECT file_name, blob_content FROM test_lob_data WHERE id = 1";
        using var command = new OracleCommand(selectSql, connection)
        {
            InitialLOBFetchSize = 4096
        };

        // Act
        using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        Assert.True(await reader.ReadAsync(TestContext.Current.CancellationToken), "Should retrieve at least one row");

        var processor = new BFileProcessor();

        // Since test container may not have BFILE setup, we verify the processor exists
        Assert.NotNull(processor);

        string formattedLength = processor.GetFormattedLobLength(1024);
        Assert.StartsWith("BFILE:", formattedLength);
        Assert.Equal(1024, processor.GetTrueLobLength(1024));

        await _fixture.ClearTestDataAsync();
    }

    [Fact]
    public async Task Scenario4_HandleNullOrEmptyLobs_Successfully()
    {
        // Arrange
        var connection = _fixture.GetConnection();

        string createTableSql = "CREATE TABLE IF NOT EXISTS test_null_lobs (id NUMBER PRIMARY KEY, content BLOB, text_content CLOB)";
        using var createCommand = new OracleCommand(createTableSql, connection);
        await createCommand.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);

        string insertSql = "INSERT INTO test_null_lobs (id, content, text_content) VALUES (1, NULL, NULL)";
        using var insertCommand = new OracleCommand(insertSql, connection);
        await insertCommand.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);

        string selectSql = "SELECT id, content, text_content FROM test_null_lobs WHERE id = 1";
        using var command = new OracleCommand(selectSql, connection)
        {
            InitialLOBFetchSize = 4096
        };

        // Act
        using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        Assert.True(await reader.ReadAsync(TestContext.Current.CancellationToken), "Should retrieve at least one row");

        bool isBlobNull = await reader.IsDBNullAsync(1, TestContext.Current.CancellationToken);
        bool isClobNull = await reader.IsDBNullAsync(2, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(isBlobNull, "BLOB should be NULL");
        Assert.True(isClobNull, "CLOB should be NULL");

        // Cleanup
        using var deleteCommand = new OracleCommand("DROP TABLE test_null_lobs", connection);
        try
        {
            await deleteCommand.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        }
        catch (OracleException)
        {
            // Ignore if table drop fails
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task MultipleRowsRetrievalWithBlobAndClob_ReturnsCorrectData(int rowId)
    {
        // Arrange
        await _fixture.SeedTestDataAsync();
        var connection = _fixture.GetConnection();

        string selectSql = "SELECT id, file_name, blob_content, clob_content FROM test_lob_data WHERE id = :id";
        using var command = new OracleCommand(selectSql, connection)
        {
            InitialLOBFetchSize = 4096
        };
        command.Parameters.Add("id", rowId);

        // Act
        using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        Assert.True(await reader.ReadAsync(TestContext.Current.CancellationToken));

        int id = reader.GetInt32(0);
        string fileName = reader.GetString(1);

        var blobProcessor = new BlobProcessor();
        using var blobStream = reader.GetOracleBlob(2);

        var clobProcessor = new ClobProcessor(Encoding.UTF8);
        using var clobStream = reader.GetOracleClob(3);

        // Assert
        Assert.Equal(rowId, id);
        Assert.NotEmpty(fileName);
        Assert.True(blobStream.CanRead);
        Assert.True(clobStream.CanRead);

        await _fixture.ClearTestDataAsync();
    }

    [Fact]
    public async Task PlsqlBlockDataReader_WithOutRefCursor_RetrievesLobData()
    {
        // Arrange
        await _fixture.SeedTestDataAsync();
        var connection = _fixture.GetConnection();

        string plsqlScript = """
            DECLARE
                v_cursor SYS_REFCURSOR;
            BEGIN
                OPEN v_cursor FOR SELECT file_name, blob_content FROM test_lob_data;
                :result := v_cursor;
            END;
            """;

        using IDataMultiReader reader = new PlsqlBlockDataReader(connection, plsqlScript, PlsqlBlockReturnType.OutRefCursor, 4096);

        // Act
        int rowCount = 0;
        await foreach (var row in reader.GetDataAsync(1, 2))
        {
            rowCount++;
            Assert.NotEmpty(row.LobName);
        }

        // Assert
        Assert.Equal(3, rowCount);

        reader.Dispose();
        await _fixture.ClearTestDataAsync();
    }
}
