#pragma warning disable CA2000 // Dispose objects before losing scope
namespace NoP77svk.OraLobUnload.DataReaders;

using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

public class PlsqlBlockDataReader : IDataMultiReader
{
    private readonly OracleConnection _dbConnection;
    private readonly string _plsqlScript;
    private readonly PlsqlBlockReturnType _plsqlBlockReturnType;
    private readonly ICollection<OracleDataReader> _dataReaders = new List<OracleDataReader>();
    private readonly ICollection<OracleCommand> _dbCommands = new List<OracleCommand>();
    private readonly int _initialLobFetchSize;
    private bool _disposedValue;

    public PlsqlBlockDataReader(OracleConnection dbConnection, string plsqlScript, PlsqlBlockReturnType plsqlBlockReturnType, int initialLobFetchSize)
    {
        _dbConnection = dbConnection;
        _plsqlBlockReturnType = plsqlBlockReturnType;
        _plsqlScript = plsqlScript.Trim().Trim('/').Trim();
        _initialLobFetchSize = initialLobFetchSize;
    }

    public IEnumerable<OracleDataReader> GetDataReaders()
    {
        OracleParameter outCursor = new OracleParameter("result", OracleDbType.RefCursor, ParameterDirection.Output);
        OracleCommand dbCommand = new OracleCommand(_plsqlScript, _dbConnection)
        {
            BindByName = false,
            CommandType = CommandType.Text,
            InitialLOBFetchSize = _initialLobFetchSize
        };

        _dbCommands.Add(dbCommand);

        if (_plsqlBlockReturnType.HasFlag(PlsqlBlockReturnType.OutRefCursor))
        {
            dbCommand.Parameters.Add(outCursor);
        }

        dbCommand.ExecuteNonQuery();

        if (_plsqlBlockReturnType.HasFlag(PlsqlBlockReturnType.OutRefCursor))
        {
            OracleDataReader result = ((OracleRefCursor)outCursor.Value).GetDataReader();
            _dataReaders.Add(result);
            yield return result;
        }

        if (_plsqlBlockReturnType.HasFlag(PlsqlBlockReturnType.ImplicitCursors))
        {
            foreach (OracleRefCursor implicitCursor in dbCommand.ImplicitRefCursors)
            {
                OracleDataReader result = implicitCursor.GetDataReader();
                _dataReaders.Add(result);
                yield return result;
            }
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                foreach (OracleDataReader dataReader in _dataReaders)
                {
                    dataReader.Dispose();
                }

                foreach (OracleCommand oracleCommand in _dbCommands)
                {
                    oracleCommand.Dispose();
                }
            }

            _disposedValue = true;
        }
    }
}

[Flags]
public enum PlsqlBlockReturnType
{
    OutRefCursor = 1,
    ImplicitCursors = 2
}
