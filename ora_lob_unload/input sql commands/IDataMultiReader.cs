namespace NoP77svk.OraLobUnload.InputSqlCommands;

using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;

internal interface IDataMultiReader : IDisposable
{
    public IEnumerable<OracleDataReader> CreateDataReaders();
}
