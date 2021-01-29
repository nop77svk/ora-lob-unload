namespace OraLobUnload
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;

    internal class InputSqlCommandFactory : IDisposable
    {
        private readonly OracleConnection _dbConnection;
        private readonly int _initialLobFetchSize;
        private ICollection<OracleCommand> _dbCommands;

        internal InputSqlCommandFactory(OracleConnection dbConnection, int initialLobFetchSize)
        {
            _dbConnection = dbConnection;
            _initialLobFetchSize = initialLobFetchSize;
            _dbCommands = new List<OracleCommand>();
        }

        public void Dispose()
        {
            foreach (OracleCommand dbCommand in _dbCommands)
                dbCommand.Dispose();
        }

        // 2do! rework to IEnumerable<ValueTuple<OracleCommand, int fileNameColumnIndex, int lobColumnIndex>> to allow for variable column indices per each table supplied
        // 2do! optionally, make the "table(s)" input type JSON-specified
        // 2do! add the remaining InputSqlReturnType's
        internal IEnumerable<OracleDataReader> CreateDataReaders(InputSqlReturnType returnType, TextReader inputSql)
        {
            IEnumerable<OracleDataReader> result = returnType switch
            {
                InputSqlReturnType.Table => CreateReadersTable(inputSql),
                InputSqlReturnType.RefCursor => CreateReadersOutputRefCursor(inputSql),
                _ => throw new NotImplementedException($"Using input script type \"{returnType}\" not (yet) implemented!")
            };

            return result;
        }

        private IEnumerable<OracleDataReader> CreateReadersOutputRefCursor(TextReader inputPlsqlStream)
        {
            OracleParameter outCursor = new OracleParameter("result", OracleDbType.RefCursor, ParameterDirection.Output); // 2do! disposable!

            string inputPlsqlBlock = inputPlsqlStream.ReadToEnd();
            OracleCommand dbCommand = new OracleCommand(inputPlsqlBlock, _dbConnection)
            {
                BindByName = false,
                CommandText = inputPlsqlBlock,
                CommandType = CommandType.Text,
                Parameters = { outCursor },
                InitialLOBFetchSize = _initialLobFetchSize
            };
            _dbCommands.Add(dbCommand);

            dbCommand.ExecuteNonQuery();
            OracleDataReader result = ((OracleRefCursor)outCursor.Value).GetDataReader();
            yield return result;
        }

        private IEnumerable<OracleDataReader> CreateReadersTable(TextReader streamOfTableNames)
        {
            string? tableName;
            while ((tableName = streamOfTableNames.ReadLine()) != null)
            {
                string cleanedUpTableName = tableName.Trim().ToUpper();
                if (cleanedUpTableName == "")
                    continue;

                OracleCommand dbCommand = new OracleCommand(cleanedUpTableName, _dbConnection)
                {
                    CommandType = CommandType.TableDirect,
                    FetchSize = 100,
                    InitialLOBFetchSize = _initialLobFetchSize
                };
                _dbCommands.Add(dbCommand);

                OracleDataReader result = dbCommand.ExecuteReader(CommandBehavior.SequentialAccess);
                yield return result;
            }
        }
    }
}
