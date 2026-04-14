namespace NoP77svk.OraLobUnload.StreamColumnProcessors;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NoP77svk.OraLobUnload.Utilities;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

public class ClobProcessor : IStreamColumnProcessor
{
    private readonly Encoding _outputEncoding;
    private OracleClob? _lobStream;
    private bool _disposedValue;

    public ClobProcessor(Encoding outputEncoding)
    {
        _outputEncoding = outputEncoding;
    }

    public Stream OpenLob(OracleDataReader dataReader, int fieldIndex)
    {
        _lobStream = dataReader.GetOracleClob(fieldIndex);
        return _lobStream;
    }

    public long GetTrueLobLength(long reportedLength)
    {
        return reportedLength / 2;
    }

    public string GetFormattedLobLength(long reportedLength)
    {
        return $"CLOB:{GetTrueLobLength(reportedLength)} characters";
    }

    public void SaveLobToStream(Stream inLob, Stream outFile)
    {
        if (inLob is not OracleClob)
        {
            throw new ArgumentException($"Must be OracleClob, is {inLob.GetType().FullName}", nameof(inLob));
        }

        var inClob = (OracleClob)inLob;

        var utf16decoder = new UnicodeEncoding(false, false);
        using var unicodeEncodingTransform = new UnicodeToAnyEncodingTransform(utf16decoder, _outputEncoding);
        using var transcoder = new CryptoStream(outFile, unicodeEncodingTransform, CryptoStreamMode.Write, true);

        inClob.CopyTo(transcoder);
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
                _lobStream?.Dispose();
            }

            _disposedValue = true;
        }
    }
}
