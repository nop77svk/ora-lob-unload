namespace NoP77svk.OraLobUnload.InputSqlCommands
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;

    internal class PlsqlBlockDataReader : IDataMultiReader
    {
        private readonly OracleConnection _dbConnection;
        private readonly string _plsqlScript;
        private readonly bool _useOutRefCursor;
        private readonly bool _useImplicitCursors;
        private readonly ICollection<OracleDataReader> _dataReaders;
        private readonly int _initialLobFetchSize;

        private OracleCommand? _dbCommand;
        private OracleParameter? _outCursor;

        internal PlsqlBlockDataReader(OracleConnection dbConnection, string plsqlScript, bool useOutRefCursor, bool useImplicitCursors, int initialLobFetchSize)
        {
            if (!useImplicitCursors && !useOutRefCursor)
                throw new ArgumentException("Must use at least one out ref cursor return type");

            _dbConnection = dbConnection;
            _plsqlScript = plsqlScript.Trim().Trim('/').Trim();
            _useImplicitCursors = useImplicitCursors;
            _useOutRefCursor = useOutRefCursor;
            _dataReaders = new List<OracleDataReader>();
            _initialLobFetchSize = initialLobFetchSize;
        }

        public IEnumerable<OracleDataReader> CreateDataReaders()
        {
            if (_useOutRefCursor)
                _outCursor = new OracleParameter("result", OracleDbType.RefCursor, ParameterDirection.Output);

            _dbCommand = new OracleCommand(_plsqlScript, _dbConnection)
            {
                BindByName = false,
                CommandType = CommandType.Text,
                InitialLOBFetchSize = _initialLobFetchSize
            };

            if (_outCursor is not null)
                _dbCommand.Parameters.Add(_outCursor);

            _dbCommand.ExecuteNonQuery();
            if (_outCursor is not null)
            {
                OracleDataReader result = ((OracleRefCursor)_outCursor.Value).GetDataReader();
                _dataReaders.Add(result);
                yield return result;
            }

            if (_useImplicitCursors)
            {
                foreach (OracleRefCursor implicitCursor in _dbCommand.ImplicitRefCursors)
                {
                    OracleDataReader result = implicitCursor.GetDataReader();
                    _dataReaders.Add(result);
                    yield return result;
                }
            }
        }

        public void Dispose()
        {
            foreach (OracleDataReader dataReader in _dataReaders)
                dataReader.Dispose();

            if (_dbCommand is not null)
                _dbCommand.Dispose();

            if (_outCursor is not null)
                _outCursor.Dispose();
        }
    }
}
