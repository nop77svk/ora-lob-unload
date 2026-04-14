namespace NoP77svk.OraLobUnload.DataReaders;

using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;

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
