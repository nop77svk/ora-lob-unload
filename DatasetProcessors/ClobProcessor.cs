namespace OraLobUnload.DatasetProcessors
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;

    internal class ClobProcessor : IDataReaderToStream
    {
        private readonly Encoding _outputEncoding;

        internal ClobProcessor(Encoding outputEncoding)
        {
            _outputEncoding = outputEncoding;
        }

        Stream IDataReaderToStream.ReadLob(OracleDataReader dataReader, int fieldIndex)
        {
            return dataReader.GetOracleClob(fieldIndex);
        }

        long IDataReaderToStream.GetTrueLobLength(long reportedLength)
        {
            return reportedLength / 2;
        }

        void IDataReaderToStream.SaveLobToStream(Stream inLob, Stream outFile)
        {
            if (inLob is not OracleClob)
                throw new ArgumentException($"Must be OracleClob, is {inLob.GetType().FullName}", nameof(inLob));

            var inClob = (OracleClob)inLob;

            var utf16decoder = new UnicodeEncoding(false, false);
            using var transcoder = new CryptoStream(outFile, new CharsetEncoderForClob(utf16decoder, _outputEncoding), CryptoStreamMode.Write, true);
            inClob.CorrectlyCopyTo(transcoder);
        }
    }
}
