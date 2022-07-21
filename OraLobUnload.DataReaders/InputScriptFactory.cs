namespace NoP77svk.OraLobUnload.DataReaders;

using System;
using System.Collections.Generic;
using System.IO;
using Oracle.ManagedDataAccess.Client;

public class InputScriptFactory
{
    private readonly OracleConnection _dbConnection;
    private readonly int _initialLobFetchSize;

    public InputScriptFactory(OracleConnection dbConnection, int initialLobFetchSize)
    {
        _dbConnection = dbConnection;
        _initialLobFetchSize = initialLobFetchSize;
    }

    public static string GetInputSqlReturnTypeDesc(InputContentType returnType)
    {
        string result = returnType switch
        {
            InputContentType.Tables => "list of tables",
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
            InputContentType.Tables => new TableDataReader(_dbConnection, SplitInputSqlToLines(inputSql), _initialLobFetchSize),
            InputContentType.OutRefCursor => new PlsqlBlockDataReader(_dbConnection, inputSql.ReadToEnd(), true, false, _initialLobFetchSize),
            InputContentType.ImplicitCursors => new PlsqlBlockDataReader(_dbConnection, inputSql.ReadToEnd(), false, true, _initialLobFetchSize),
            InputContentType.Select => new SqlQueryDataReader(_dbConnection, inputSql.ReadToEnd(), _initialLobFetchSize),
            _ => throw new NotImplementedException($"Using input script type \"{returnType}\" not (yet) implemented!")
        };

        return result;
    }

    private static IEnumerable<string> SplitInputSqlToLines(TextReader inputSql)
    {
        string? tableName;
        while ((tableName = inputSql.ReadLine()) != null)
        {
            string cleanedUpTableName = tableName.Trim().ToUpper();
            if (cleanedUpTableName != "")
                yield return cleanedUpTableName;
        }
    }
}
