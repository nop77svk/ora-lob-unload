#pragma warning disable CA2000 // Dispose objects before losing scope
namespace NoP77svk.OraLobUnload.DataReaders;

using System.Collections.Generic;
using System.Data;

using Oracle.ManagedDataAccess.Client;

public class TableDataReader : IDataMultiReader
{
    private readonly OracleConnection _dbConnection;
    private readonly IEnumerable<string> _tableNames;
    private readonly ICollection<OracleCommand> _dbCommands;
    private readonly ICollection<OracleDataReader> _dataReaders;
    private readonly int _initialLobFetchSize;
    private bool _disposedValue;

    public TableDataReader(OracleConnection dbConnection, IEnumerable<string> tableNames, int initialLobFetchSize)
    {
        _dbConnection = dbConnection;
        _tableNames = tableNames;
        _dbCommands = new List<OracleCommand>();
        _dataReaders = new List<OracleDataReader>();
        _initialLobFetchSize = initialLobFetchSize;
    }

    public IEnumerable<OracleDataReader> GetDataReaders()
    {
        foreach (string tableName in _tableNames)
        {
            string cleanedUpTableName = tableName.Trim().ToUpper();
            if (string.IsNullOrEmpty(cleanedUpTableName))
            {
                continue;
            }

            OracleCommand command = new OracleCommand(cleanedUpTableName, _dbConnection)
            {
                CommandType = CommandType.TableDirect,
                FetchSize = 100,
                InitialLOBFetchSize = _initialLobFetchSize
            };

            _dbCommands.Add(command);

            OracleDataReader result = command.ExecuteReader(CommandBehavior.SequentialAccess);
            _dataReaders.Add(result);

            yield return result;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                foreach (OracleDataReader dataReader in _dataReaders)
                {
                    dataReader.Dispose();
                }

                foreach (OracleCommand oracleCommand in _dbCommands)
                {
                    oracleCommand.Dispose();
                }
            }

            _disposedValue = true;
        }
    }
}
