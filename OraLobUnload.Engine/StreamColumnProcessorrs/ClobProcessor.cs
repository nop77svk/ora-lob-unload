namespace NoP77svk.OraLobUnload.Engine.StreamColumnProcessors;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using NoP77svk.OraLobUnload.Engine.Infrastructure;

using Oracle.ManagedDataAccess.Types;

public class ClobProcessor : IStreamColumnProcessor
{
    private readonly Encoding _outputEncoding;

    public ClobProcessor(Encoding outputEncoding)
    {
        _outputEncoding = outputEncoding;
    }

    public long GetTrueLobLength(long reportedLength)
    {
        return reportedLength / 2;
    }

    public string GetFormattedLobLength(long reportedLength)
    {
        return $"CLOB:{GetTrueLobLength(reportedLength)} characters";
    }

    public async Task SaveLobToStreamAsync(Stream inLob, Stream outFile)
    {
        if (inLob is not OracleClob inClob)
        {
            throw new ArgumentException($"Must be OracleClob, is {inLob.GetType().FullName}", nameof(inLob));
        }

        var utf16decoder = new UnicodeEncoding(false, false);
        using var unicodeEncodingTransform = new UnicodeToAnyEncodingTransform(utf16decoder, _outputEncoding);
        using var transcoder = new CryptoStream(outFile, unicodeEncodingTransform, CryptoStreamMode.Write, true);

        await inClob.CopyToAsync(transcoder);
    }
}
