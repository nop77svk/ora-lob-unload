namespace NoP77svk.OraLobUnload.InputSqlCommands
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;

    internal class SqlQueryDataReader : IDataMultiReader
    {
        private readonly OracleConnection _dbConnection;
        private readonly string _sqlQuery;
        private readonly ICollection<OracleDataReader> _dataReaders;
        private readonly int _initialLobFetchSize;

        private OracleCommand? _dbCommand;

        internal SqlQueryDataReader(OracleConnection dbConnection, string sqlQuery, int initialLobFetchSize)
        {
            _dbConnection = dbConnection;
            _sqlQuery = sqlQuery.Trim().Trim(';').Trim();
            _dataReaders = new List<OracleDataReader>();
            _initialLobFetchSize = initialLobFetchSize;
        }

        public IEnumerable<OracleDataReader> CreateDataReaders()
        {
            _dbCommand = new OracleCommand(_sqlQuery, _dbConnection)
            {
                BindByName = false,
                CommandType = CommandType.Text,
                InitialLOBFetchSize = _initialLobFetchSize
            };

            OracleDataReader result = _dbCommand.ExecuteReader();
            _dataReaders.Add(result);
            yield return result;
        }

        public void Dispose()
        {
            foreach (OracleDataReader dataReader in _dataReaders)
                dataReader.Dispose();

            if (_dbCommand is not null)
                _dbCommand.Dispose();
        }
    }
}
