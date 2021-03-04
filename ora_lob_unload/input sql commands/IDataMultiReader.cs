namespace OraLobUnload.InputSqlCommands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Oracle.ManagedDataAccess.Client;

    internal interface IDataMultiReader : IDisposable
    {
        public IEnumerable<OracleDataReader> CreateDataReaders();
    }
}
