namespace OraLobUnload.StreamColumnProcessors
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using nop77svk.lib.StreamProcessors;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;

    internal class ClobProcessor : IStreamColumnProcessor
    {
        private readonly Encoding _outputEncoding;

        public ClobProcessor(Encoding outputEncoding)
        {
            _outputEncoding = outputEncoding;
        }

        public Stream ReadLob(OracleDataReader dataReader, int fieldIndex)
        {
            return dataReader.GetOracleClob(fieldIndex);
        }

        public long GetTrueLobLength(long reportedLength)
        {
            return reportedLength / 2;
        }

        public string GetFormattedLobLength(long reportedLength)
        {
            return $"{GetTrueLobLength(reportedLength)} characters long CLOB";
        }

        public void SaveLobToStream(Stream inLob, Stream outFile)
        {
            if (inLob is not OracleClob)
                throw new ArgumentException($"Must be OracleClob, is {inLob.GetType().FullName}", nameof(inLob));

            var inClob = (OracleClob)inLob;

            var utf16decoder = new UnicodeEncoding(false, false);
            using var transcoder = new CryptoStream(outFile, new UnicodeToAnyEncodingTransform(utf16decoder, _outputEncoding), CryptoStreamMode.Write, true);
            inClob.CorrectlyCopyTo(transcoder);
        }
    }
}
