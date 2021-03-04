namespace OraLobUnload.InputSqlCommands
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

        // 2do! rework to IEnumerable<ValueTuple<OracleCommand, int fileNameColumnIndex, int lobColumnIndex>> to allow for variable column indices per each table supplied
        // 2do! optionally, make the "table(s)" input type JSON-specified
        // 2do! add the remaining InputSqlReturnType's
        internal IDataMultiReader CreateMultiReader(InputSqlReturnType returnType, TextReader inputSql)
        {
            IDataMultiReader result = returnType switch
            {
                InputSqlReturnType.Table => new TableDataReader(_dbConnection, SplitInputSqlToLines(inputSql), _initialLobFetchSize),
                InputSqlReturnType.RefCursor => new PlsqlBlockDataReader(_dbConnection, inputSql.ReadToEnd(), true, false, _initialLobFetchSize),
                InputSqlReturnType.MultiImplicitCursors => new PlsqlBlockDataReader(_dbConnection, inputSql.ReadToEnd(), false, true, _initialLobFetchSize),
                _ => throw new NotImplementedException($"Using input script type \"{returnType}\" not (yet) implemented!")
            };

            return result;
        }

        private IEnumerable<string> SplitInputSqlToLines(TextReader inputSql)
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
