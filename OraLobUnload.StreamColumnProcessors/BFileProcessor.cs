namespace NoP77svk.OraLobUnload.StreamColumnProcessors;

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

    public void SaveLobToStream(Stream inLob, Stream outFile)
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
            oracleBFile.CopyTo(outFile);
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
