namespace OraLobUnload
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Oracle.ManagedDataAccess.Client;

    internal class InputSqlCommandFactory
    {
        private readonly OracleConnection _dbConnection;

        internal InputSqlCommandFactory(OracleConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        internal IEnumerable<OracleCommand> CreateDbCommands(InputSqlReturnType returnType, TextReader inputReader, IEnumerable<string>? inputArguments)
        {
            IEnumerable<OracleCommand> result = returnType switch
            {
                InputSqlReturnType.Table => CreateCommandTable(inputReader),
                _ => throw new NotImplementedException($"Using input script type \"{returnType}\" not (yet) implemented!")
            };

            return result;
        }

        internal IEnumerable<OracleCommand> CreateCommandTable(TextReader streamOfTableNames)
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
