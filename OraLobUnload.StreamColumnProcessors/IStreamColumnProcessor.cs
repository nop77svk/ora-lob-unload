namespace NoP77svk.OraLobUnload.StreamColumnProcessors;

using System;
using System.IO;
using Oracle.ManagedDataAccess.Client;

public interface IStreamColumnProcessor : IDisposable
{
    public Stream OpenLob(OracleDataReader dataReader, int fieldIndex);

    public long GetTrueLobLength(long reportedLength);

    public string GetFormattedLobLength(long reportedLength);

    public void SaveLobToStream(Stream inLob, Stream outFile);
}
