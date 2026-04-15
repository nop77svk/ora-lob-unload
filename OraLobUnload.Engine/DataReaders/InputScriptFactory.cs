namespace NoP77svk.OraLobUnload.Engine.DataReaders;

using System;
using System.Collections.Generic;
using System.IO;
using Oracle.ManagedDataAccess.Client;

public class InputScriptFactory
{
    public int InitialLobFetchSize { get; init; } = 1048576;

    private readonly OracleConnection _dbConnection;

    public InputScriptFactory(OracleConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public static string GetInputSqlReturnTypeDesc(InputContentType returnType)
    {
        string result = returnType switch
        {
            InputContentType.OutRefCursor => "PL/SQL block with output ref cursor",
            InputContentType.ImplicitCursors => "PL/SQL block with implicit output cursors",
            InputContentType.Select => "SQL query",
            _ => throw new NotImplementedException($"Using input script type \"{returnType}\" not (yet) implemented!")
        };

        return result;
    }

    // 2do! rework to IEnumerable<ValueTuple<OracleCommand, int fileNameColumnIndex, int lobColumnIndex>> to allow for variable column indices per each table supplied
    // 2do! optionally, make the "table(s)" input type JSON-specified
    // 2do! add the remaining InputSqlReturnType's
    public IDataMultiReader CreateMultiReader(InputContentType returnType, TextReader inputSql)
    {
        IDataMultiReader result = returnType switch
        {
            InputContentType.OutRefCursor => new PlsqlBlockDataReader(_dbConnection, inputSql.ReadToEnd(), PlsqlBlockReturnType.OutRefCursor, InitialLobFetchSize),
            InputContentType.ImplicitCursors => new PlsqlBlockDataReader(_dbConnection, inputSql.ReadToEnd(), PlsqlBlockReturnType.ImplicitCursors, InitialLobFetchSize),
            InputContentType.Select => new SqlQueryDataReader(_dbConnection, inputSql.ReadToEnd(), InitialLobFetchSize),
            _ => throw new NotImplementedException($"Using input script type \"{returnType}\" not (yet) implemented!")
        };

        return result;
    }
}
