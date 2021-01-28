namespace OraLobUnload
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using Oracle.ManagedDataAccess.Client;

    internal class InputSqlCommandFactory
    {
        private readonly OracleConnection _dbConnection;

        internal InputSqlCommandFactory(OracleConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // 2do! rework to IEnumerable<ValueTuple<OracleCommand, int fileNameColumnIndex, int lobColumnIndex>> to allow for variable column indices per each table supplied
        // 2do! optionally, make the "table(s)" input type JSON-specified
        // 2do! add the remaining InputSqlReturnType's
        internal IEnumerable<OracleCommand> CreateDbCommands(InputSqlReturnType returnType, TextReader inputReader)
        {
            IEnumerable<OracleCommand> result = returnType switch
            {
                InputSqlReturnType.Table => CreateCommandTable(inputReader),
                _ => throw new NotImplementedException($"Using input script type \"{returnType}\" not (yet) implemented!")
            };

            return result;
        }

        private IEnumerable<OracleCommand> CreateCommandTable(TextReader streamOfTableNames)
        {
            string? tableName;
            while ((tableName = streamOfTableNames.ReadLine()) != null)
            {
                string cleanedUpTableName = tableName.Trim().ToUpper();
                if (cleanedUpTableName == "")
                    continue;

                Console.WriteLine($"Reading data from table \"{cleanedUpTableName}\"");

                OracleCommand result = new OracleCommand(cleanedUpTableName, _dbConnection)
                {
                    CommandType = System.Data.CommandType.TableDirect,
                    FetchSize = 100,
                    InitialLOBFetchSize = 262144
                };

                yield return result;
            }
        }
    }
}
