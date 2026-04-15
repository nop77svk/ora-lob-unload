#pragma warning disable CA2000 // Dispose objects before losing scope
namespace NoP77svk.OraLobUnload.Engine.DataReaders;

using System;
using System.Collections.Generic;
using System.Data;

using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

public class PlsqlBlockDataReader : IDataMultiReader
{
    private readonly OracleConnection _dbConnection;
    private readonly string _plsqlScript;
    private readonly PlsqlBlockReturnType _plsqlBlockReturnType;
    private readonly int _initialLobFetchSize;

    public PlsqlBlockDataReader(OracleConnection dbConnection, string plsqlScript, PlsqlBlockReturnType plsqlBlockReturnType, int initialLobFetchSize)
    {
        _dbConnection = dbConnection;
        _plsqlBlockReturnType = plsqlBlockReturnType;
        _plsqlScript = plsqlScript.Trim().Trim('/').Trim();
        _initialLobFetchSize = initialLobFetchSize;
    }

    public async IAsyncEnumerable<DataMultiReaderRow> GetDataAsync(int fieldNameIndex, int fieldValueIndex)
    {
        using OracleParameter outCursor = new OracleParameter("result", OracleDbType.RefCursor, ParameterDirection.Output);

        using OracleCommand dbCommand = new OracleCommand(_plsqlScript, _dbConnection)
        {
            BindByName = false,
            CommandType = CommandType.Text,
            InitialLOBFetchSize = _initialLobFetchSize
        };

        if (_plsqlBlockReturnType.HasFlag(PlsqlBlockReturnType.OutRefCursor))
        {
            dbCommand.Parameters.Add(outCursor);
        }

        await dbCommand.ExecuteNonQueryAsync();

        if (_plsqlBlockReturnType.HasFlag(PlsqlBlockReturnType.OutRefCursor))
        {
            using OracleDataReader reader = ((OracleRefCursor)outCursor.Value).GetDataReader();
            await foreach (var kvp in IDataMultiReader.FetchDataFromReaderAsync(fieldNameIndex, fieldValueIndex, reader))
            {
                yield return kvp;
            }
        }

        if (_plsqlBlockReturnType.HasFlag(PlsqlBlockReturnType.ImplicitCursors))
        {
            foreach (OracleRefCursor implicitCursor in dbCommand.ImplicitRefCursors)
            {
                using OracleDataReader reader = implicitCursor.GetDataReader();
                await foreach (var kvp in IDataMultiReader.FetchDataFromReaderAsync(fieldNameIndex, fieldValueIndex, reader))
                {
                    yield return kvp;
                }
            }
        }
    }
}

[Flags]
public enum PlsqlBlockReturnType
{
    OutRefCursor = 1,
    ImplicitCursors = 2
}
