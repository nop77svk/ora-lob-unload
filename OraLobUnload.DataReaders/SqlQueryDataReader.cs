namespace NoP77svk.OraLobUnload.DataReaders;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

public class SqlQueryDataReader : IDataMultiReader
{
    private readonly ICollection<OracleDataReader> _dataReaders;
    private readonly int _initialLobFetchSize;

    private readonly OracleCommand _dbCommand;
    private bool _disposedValue;

    public string SqlQuery { get; }

    public SqlQueryDataReader(OracleConnection dbConnection, string sqlQuery, int initialLobFetchSize)
    {
        SqlQuery = sqlQuery.Trim().Trim(';').Trim();
        _dbCommand = new OracleCommand(SqlQuery, dbConnection)
        {
            BindByName = false,
            CommandType = CommandType.Text,
            InitialLOBFetchSize = _initialLobFetchSize
        };

        _dataReaders = new List<OracleDataReader>();
        _initialLobFetchSize = initialLobFetchSize;
    }

    public async IAsyncEnumerable<DataMultiReaderRow> GetDataAsync(int fieldNameIndex, int fieldValueIndex)
    {
        using OracleDataReader reader = await _dbCommand.ExecuteReaderAsync();
        await foreach (var kvp in IDataMultiReader.FetchDataFromReaderAsync(fieldNameIndex, fieldValueIndex, reader))
        {
            yield return kvp;
        }
    }

    public IEnumerable<OracleDataReader> GetDataReaders()
    {
        OracleDataReader result = _dbCommand.ExecuteReader();
        _dataReaders.Add(result);
        yield return result;
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

                _dbCommand.Dispose();
            }

            _disposedValue = true;
        }
    }
}
