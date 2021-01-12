namespace OraLobUnload
{
    using Oracle.ManagedDataAccess.Client;
    using System.Collections.Generic;
    using System;
    using System.Threading.Tasks;
    using System.Data.Common;

    class InputSqlCommandFactory
    {
        private readonly OracleConnection dbConnection;

        public InputSqlCommandFactory(OracleConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        public IEnumerable<DbDataReader> GetResultReaders(InputSqlReturnTypeEnum returnType, string command, IEnumerable<string> inputArguments)
        {
            var result = returnType switch
            {
                InputSqlReturnTypeEnum.Table => CreateCommandTable(command),
                _ => throw new NotImplementedException($"Using input script type \"{returnType}\" not (yet) implemented!")
            };

            return result;
        }

        private IEnumerable<DbDataReader> CreateCommandTable(string command)
        {
            string cleanedUpTableName = command.Trim().ToUpper();
            Console.WriteLine($"Reading data from table \"{cleanedUpTableName}\"");

            OracleCommand result = new OracleCommand(cleanedUpTableName, this.dbConnection)
            {
                CommandType = System.Data.CommandType.TableDirect,
                FetchSize = 100,
                InitialLOBFetchSize = 262144
            };
            return new List<DbDataReader>() {
                result.ExecuteReader(System.Data.CommandBehavior.Default)
            };
        }
    }
}
