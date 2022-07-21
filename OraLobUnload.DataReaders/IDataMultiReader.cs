namespace NoP77svk.OraLobUnload.DataReaders;

using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;

public interface IDataMultiReader : IDisposable
{
    public IEnumerable<OracleDataReader> CreateDataReaders();
}
