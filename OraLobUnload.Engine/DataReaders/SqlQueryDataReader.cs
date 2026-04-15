namespace NoP77svk.OraLobUnload.Engine.DataReaders;

using System.Collections.Generic;
using System.Data;

using Oracle.ManagedDataAccess.Client;

public class SqlQueryDataReader : IDataMultiReader
{
    private readonly int _initialLobFetchSize;
    private readonly OracleCommand _dbCommand;

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

        _initialLobFetchSize = initialLobFetchSize;
    }

    public async IAsyncEnumerable<DataMultiReaderRow> GetDataAsync(int fieldNameIndex, int fieldValueIndex)
    {
        using OracleDataReader reader = (OracleDataReader)await _dbCommand.ExecuteReaderAsync();
        await foreach (var kvp in IDataMultiReader.FetchDataFromReaderAsync(fieldNameIndex, fieldValueIndex, reader))
        {
            yield return kvp;
        }
    }
}
