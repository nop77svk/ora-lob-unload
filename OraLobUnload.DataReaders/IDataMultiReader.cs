namespace NoP77svk.OraLobUnload.DataReaders;

using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;

public interface IDataMultiReader : IDisposable
{
    [Obsolete("Use GetData() instead!")]
    IEnumerable<OracleDataReader> GetDataReaders();
    IAsyncEnumerable<KeyValuePair<string, object>> GetDataAsync(int fieldNameIndex, int fieldValueIndex);

    protected static async IAsyncEnumerable<KeyValuePair<string, object>> FetchDataFromReaderAsync(int lobNameIndex, int lobValueIndex, OracleDataReader reader)
    {
        while (await reader.ReadAsync())
        {
            string lobName = reader.GetString(lobNameIndex);
            object? lobValue = reader.GetValue(lobValueIndex);
            yield return new KeyValuePair<string, object>(lobName, lobValue);
        }
    }
}
