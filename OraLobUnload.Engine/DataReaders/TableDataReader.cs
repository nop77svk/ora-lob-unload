#pragma warning disable CA2000 // Dispose objects before losing scope
namespace NoP77svk.OraLobUnload.Engine.DataReaders;

using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using Oracle.ManagedDataAccess.Client;

public class TableDataReader : IDataMultiReader
{
    private readonly OracleConnection _dbConnection;
    private readonly IEnumerable<string> _tableNames;
    private readonly int _initialLobFetchSize;

    public TableDataReader(OracleConnection dbConnection, IEnumerable<string> tableNames, int initialLobFetchSize)
    {
        _dbConnection = dbConnection;
        _tableNames = tableNames;
        _initialLobFetchSize = initialLobFetchSize;
    }

    public async IAsyncEnumerable<DataMultiReaderRow> GetDataAsync(int fieldNameIndex, int fieldValueIndex)
    {
        var cleanedUpTableNames = _tableNames
            .Select(tableName => tableName.Trim().ToUpper())
            .Where(tableName => !string.IsNullOrEmpty(tableName));

        List<OracleCommand> commands = cleanedUpTableNames
            .Select(tableName => new OracleCommand(tableName, _dbConnection)
            {
                CommandType = CommandType.TableDirect,
                FetchSize = 100,
                InitialLOBFetchSize = _initialLobFetchSize
            })
            .ToList();

        try
        {
            List<Task<OracleDataReader>> readerTasks = commands
                .Select(command => command.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
                .ToList();

            await foreach (var readerTask in Task.WhenEach(readerTasks))
            {
                using OracleDataReader reader = await readerTask;
                await foreach (var kvp in IDataMultiReader.FetchDataFromReaderAsync(fieldNameIndex, fieldValueIndex, reader))
                {
                    yield return kvp;
                }
            }
        }
        finally
        {
            foreach (OracleCommand command in commands)
            {
                await command.DisposeAsync();
            }
        }
    }
}
