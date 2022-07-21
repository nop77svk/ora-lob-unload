namespace NoP77svk.OraLobUnload.InputSqlCommands
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Oracle.ManagedDataAccess.Client;

    internal class TableDataReader : IDataMultiReader
    {
        private readonly OracleConnection _dbConnection;
        private readonly IEnumerable<string> _tableNames;
        private readonly ICollection<OracleCommand> _dbCommands;
        private readonly ICollection<OracleDataReader> _dataReaders;
        private readonly int _initialLobFetchSize;

        internal TableDataReader(OracleConnection dbConnection, IEnumerable<string> tableNames, int initialLobFetchSize)
        {
            _dbConnection = dbConnection;
            _tableNames = tableNames;
            _dbCommands = new List<OracleCommand>();
            _dataReaders = new List<OracleDataReader>();
            _initialLobFetchSize = initialLobFetchSize;
        }

        public IEnumerable<OracleDataReader> CreateDataReaders()
        {
            foreach (string tableName in _tableNames)
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
                _dataReaders.Add(result);

                yield return result;
            }
        }

        void IDisposable.Dispose()
        {
            foreach (OracleDataReader dataReader in _dataReaders)
                dataReader.Dispose();

            foreach (OracleCommand dbCommand in _dbCommands)
                dbCommand.Dispose();
        }
    }
}
