namespace NoP77svk.OraLobUnload.StreamColumnProcessors;

using System;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

public class BlobProcessor : IStreamColumnProcessor
{
    private OracleBlob? _lobStream;

    public Stream ReadLob(OracleDataReader dataReader, int fieldIndex)
    {
        _lobStream = dataReader.GetOracleBlob(fieldIndex);
        return _lobStream;
    }

    public long GetTrueLobLength(long reportedLength)
    {
        return reportedLength;
    }

    public string GetFormattedLobLength(long reportedLength)
    {
        return $"BLOB:{GetTrueLobLength(reportedLength)} bytes";
    }

    public void SaveLobToStream(Stream inLob, Stream outFile)
    {
        if (inLob is not OracleBlob)
            throw new ArgumentException($"Must be OracleBlob, is {inLob.GetType().FullName}", nameof(inLob));

        inLob.CopyTo(outFile);
    }

    public void Dispose()
    {
        _lobStream?.Dispose();
    }
}
