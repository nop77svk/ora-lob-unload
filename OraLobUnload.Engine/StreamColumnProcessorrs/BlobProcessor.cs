namespace NoP77svk.OraLobUnload.Engine.StreamColumnProcessors;

using System;
using System.IO;

using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

public class BlobProcessor : IStreamColumnProcessor
{
    public long GetTrueLobLength(long reportedLength)
    {
        return reportedLength;
    }

    public string GetFormattedLobLength(long reportedLength)
    {
        return $"BLOB:{GetTrueLobLength(reportedLength)} bytes";
    }

    public async Task SaveLobToStreamAsync(Stream inLob, Stream outFile)
    {
        if (inLob is not OracleBlob inBlob)
        {
            throw new ArgumentException($"Must be OracleBlob, is {inLob.GetType().FullName}", nameof(inLob));
        }

        await inBlob.CopyToAsync(outFile);
    }
}
