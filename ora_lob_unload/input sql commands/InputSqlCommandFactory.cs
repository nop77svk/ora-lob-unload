namespace SK.NoP77svk.OraLobUnload.InputSqlCommands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Oracle.ManagedDataAccess.Client;

    internal class InputSqlCommandFactory
    {
        private readonly OracleConnection _dbConnection;
        private readonly int _initialLobFetchSize;

        internal InputSqlCommandFactory(OracleConnection dbConnection, int initialLobFetchSize)
        {
            _dbConnection = dbConnection;
            _initialLobFetchSize = initialLobFetchSize;
        }

        internal static string GetInputSqlReturnTypeDesc(InputSqlReturnType returnType)
        {
            string result = returnType switch
            {
                InputSqlReturnType.Tables => "list of tables",
                InputSqlReturnType.OutRefCursor => "PL/SQL block with output ref cursor",
                InputSqlReturnType.ImplicitCursors => "PL/SQL block with implicit output cursors",
                InputSqlReturnType.Select => "SQL query",
                _ => throw new NotImplementedException($"Using input script type \"{returnType}\" not (yet) implemented!")
            };

            return result;
        }

        // 2do! rework to IEnumerable<ValueTuple<OracleCommand, int fileNameColumnIndex, int lobColumnIndex>> to allow for variable column indices per each table supplied
        // 2do! optionally, make the "table(s)" input type JSON-specified
        // 2do! add the remaining InputSqlReturnType's
        internal IDataMultiReader CreateMultiReader(InputSqlReturnType returnType, TextReader inputSql)
        {
            IDataMultiReader result = returnType switch
            {
                InputSqlReturnType.Tables => new TableDataReader(_dbConnection, SplitInputSqlToLines(inputSql), _initialLobFetchSize),
                InputSqlReturnType.OutRefCursor => new PlsqlBlockDataReader(_dbConnection, inputSql.ReadToEnd(), true, false, _initialLobFetchSize),
                InputSqlReturnType.ImplicitCursors => new PlsqlBlockDataReader(_dbConnection, inputSql.ReadToEnd(), false, true, _initialLobFetchSize),
                InputSqlReturnType.Select => new SqlQueryDataReader(_dbConnection, inputSql.ReadToEnd(), _initialLobFetchSize),
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
}
