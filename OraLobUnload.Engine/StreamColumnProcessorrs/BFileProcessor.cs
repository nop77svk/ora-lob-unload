namespace NoP77svk.OraLobUnload.Engine.StreamColumnProcessors;

using System;
using System.IO;

using Oracle.ManagedDataAccess.Types;

public class BFileProcessor : IStreamColumnProcessor
{
    public long GetTrueLobLength(long reportedLength)
    {
        return reportedLength;
    }

    public string GetFormattedLobLength(long reportedLength)
    {
        return $"BFILE:{GetTrueLobLength(reportedLength)} bytes";
    }

    public async Task SaveLobToStreamAsync(Stream inLob, Stream outFile)
    {
        if (inLob is not OracleBFile oracleBFile)
        {
            throw new ArgumentException($"Must be OracleBFile, is {inLob.GetType().FullName}", nameof(inLob));
        }

        if (!oracleBFile.IsOpen)
        {
            oracleBFile.OpenFile();
        }

        try
        {
            await oracleBFile.CopyToAsync(outFile);
        }
        finally
        {
            if (oracleBFile.IsOpen)
            {
                oracleBFile.CloseFile();
            }
        }
    }
}
